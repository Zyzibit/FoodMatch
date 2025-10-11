using System.Text;
using System.Text.Json;
using inzynierka.AI.OpenAI;
using inzynierka.AI.OpenAI.Model;
using inzynierka.Auth.Model;
using inzynierka.Auth.Services;
using inzynierka.Auth.Repositories;
using inzynierka.Data;
using inzynierka.Products.Extensions;
using inzynierka.Products.OpenFoodFacts.Import;
using inzynierka.Products.OpenFoodFacts.Mappings;
using inzynierka.Products.OpenFoodFacts.OpenFoodFactsDeserializer.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

// New modular imports
using inzynierka.Auth.Contracts;
using inzynierka.AI.Contracts;
using inzynierka.Auth.Modules;
using inzynierka.AI.Modules;
using inzynierka.EventBus;

// gRPC Services - new modular structure
using inzynierka.Auth.Grpc.Services;
using inzynierka.Products.Grpc.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
{
    var configuration = provider.GetService<IConfiguration>();
    var logger = provider.GetService<ILogger<Program>>();
    var connectionString = configuration!.GetConnectionString("Redis") ?? "127.0.0.1:6379";
    
    var options = ConfigurationOptions.Parse(connectionString);
    
    // Connection settings
    options.AbortOnConnectFail = false;
    options.ConnectTimeout = 30000; 
    options.SyncTimeout = 30000; 
    options.AsyncTimeout = 30000; 
    options.ConnectRetry = 10; 
    options.ReconnectRetryPolicy = new ExponentialRetry(1000, 30000); 
    
    options.KeepAlive = 180; 
    options.DefaultDatabase = 0;
    
    try 
    {
        var multiplexer = ConnectionMultiplexer.Connect(options);
        
        // Log connection events
        multiplexer.ConnectionFailed += (sender, args) =>
        {
            logger?.LogError("Redis connection failed: {Exception}", args.Exception?.Message);
        };
        
        multiplexer.ConnectionRestored += (sender, args) =>
        {
            logger?.LogInformation("Redis connection restored");
        };
        
        multiplexer.ErrorMessage += (sender, args) =>
        {
            logger?.LogError("Redis error: {Message}", args.Message);
        };
        
        logger?.LogInformation("Redis connection established successfully");
        return multiplexer;
    }
    catch (Exception ex)
    {
        logger?.LogError(ex, "Failed to connect to Redis. Using fallback configuration.");
        throw;
    }
});

builder.Services.AddSingleton<IEventBus, InMemoryEventBus>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IRoleInitializationService, RoleInitializationService>();

builder.Services.AddProductsServices();

builder.Services.AddHostedService<TokenCleanupService>();

builder.Services.AddScoped<IAuthContract, AuthModule>();
builder.Services.AddScoped<IAIContract, AIModule>();

builder.Services.AddGrpc();

builder.Services.AddGrpcClient<inzynierka.Auth.Grpc.AuthService.AuthServiceClient>(options =>
{
    options.Address = new Uri("https://localhost:5001");
});
builder.Services.AddGrpcClient<inzynierka.Products.Grpc.ProductService.ProductServiceClient>(options =>
{
    options.Address = new Uri("https://localhost:5001");
});

builder.Services.AddHttpClient<IOpenAIClient,OpenAIClient>();
builder.Services.AddSingleton<OpenAIClient>();

builder.Services.AddAutoMapper(typeof(OpenFoodFactsProfile));


builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddAuthorization();
builder.Services.AddControllers();

builder.Services.AddHealthChecks()
    .AddRedis(builder.Configuration.GetConnectionString("Redis") ?? "127.0.0.1:6379");

builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }
    )
    .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidAudience = builder.Configuration["JWT:ValidAudience"],
                ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
                ClockSkew = TimeSpan.Zero,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:secret"]))
            };
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    if (context.Request.Cookies.ContainsKey("AccessToken"))
                    {
                        context.Token = context.Request.Cookies["AccessToken"];
                    }
                    return Task.CompletedTask;
                }
            };
        }
        
    )
    ;

var app = builder.Build();

app.MapHealthChecks("/health");
app.MapHealthChecks("/alive");

app.MapGrpcService<AuthGrpcService>();

app.MapGrpcService<ProductsGrpcService>();

app.UseCors(policy =>
    policy.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    var db = sp.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    var roleInit = sp.GetRequiredService<IRoleInitializationService>();
    await roleInit.InitializeRolesAsync();

    await DbSeeder.SeedData(app);
}
app.UseHttpsRedirection();

await DbSeeder.SeedData(app);  

app.Run();

