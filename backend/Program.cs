using System.Text;
using inzynierka.AI.OpenAI;
using inzynierka.AI.OpenAI.Services;
using inzynierka.Auth.Repositories;
using inzynierka.Data;
using inzynierka.Products.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using inzynierka.Auth.Services;
using inzynierka.Users.Model;
using inzynierka.Users.Services;
using inzynierka.Recipes.Repositories;
using inzynierka.Recipes.Services;
using inzynierka.Units.Repositories;
using inzynierka.Units.Services;

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
        multiplexer.ConnectionFailed += (_, args) =>
        {
            logger?.LogError("Redis connection failed: {Exception}", args.Exception?.Message);
        };
        
        multiplexer.ConnectionRestored += (_, _) =>
        {
            logger?.LogInformation("Redis connection restored");
        };
        
        multiplexer.ErrorMessage += (_, args) =>
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

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IRoleInitializationService, RoleInitializationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();

builder.Services.AddProductsServices();

builder.Services.AddHostedService<TokenCleanupService>();

builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();
builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<IUnitRepository, UnitRepository>();
builder.Services.AddScoped<IUnitService, UnitService>();
builder.Services.AddScoped<IPromptConfigService, PromptConfigService>();
builder.Services.AddScoped<IRecipeGeneratorService, RecipeGeneratorService>();

builder.Services.AddScoped<inzynierka.MealPlans.Services.IMealPlanService, inzynierka.MealPlans.Services.MealPlanService>();
builder.Services.AddScoped<inzynierka.MealPlans.Repositories.IMealPlanRepository, inzynierka.MealPlans.Repositories.MealPlanRepository>();

// Rejestracja OpenAI Client używającego oficjalnej biblioteki
builder.Services.AddScoped<IAiClient, inzynierka.AI.OpenAI.AiClient>();

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddAuthorization();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

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
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:secret"] ?? string.Empty))
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

//zmiana tutaj w corsie
app.UseCors(policy =>
    policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
        .AllowCredentials()
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
