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

// Redis Configuration with improved reliability
builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
{
    var configuration = provider.GetService<IConfiguration>();
    var logger = provider.GetService<ILogger<Program>>();
    var connectionString = configuration!.GetConnectionString("Redis") ?? "127.0.0.1:6379";
    
    var options = ConfigurationOptions.Parse(connectionString);
    
    // Connection settings
    options.AbortOnConnectFail = false; // Allow retries instead of failing immediately
    options.ConnectTimeout = 30000; // 30 seconds timeout for initial connection
    options.SyncTimeout = 30000; // 30 seconds sync timeout
    options.AsyncTimeout = 30000; // 30 seconds async timeout
    options.ConnectRetry = 10; // Retry 10 times
    options.ReconnectRetryPolicy = new ExponentialRetry(1000, 30000); // 1-30 seconds backoff
    
    // Performance settings
    options.KeepAlive = 180; // Keep alive every 3 minutes
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

// Event Bus (szyna danych)
builder.Services.AddSingleton<IEventBus, InMemoryEventBus>();

// Auth Services and Repositories
builder.Services.AddScoped<IAuthService, inzynierka.Auth.Services.AuthService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IRoleInitializationService, RoleInitializationService>();

// Products module services (using extension method)
builder.Services.AddProductsServices();

// Background Services
builder.Services.AddHostedService<TokenCleanupService>();

// Kontrakty modu³ów (g³ówne interfejsy komunikacji)
builder.Services.AddScoped<IAuthContract, AuthModule>();
builder.Services.AddScoped<IAIContract, AIModule>();

// gRPC Services (komunikacja wewnêtrzna)
builder.Services.AddGrpc();

// gRPC Clients (dla komunikacji miêdzy serwisami)
builder.Services.AddGrpcClient<inzynierka.Auth.Grpc.AuthService.AuthServiceClient>(options =>
{
    options.Address = new Uri("https://localhost:5001");
});
builder.Services.AddGrpcClient<inzynierka.Products.Grpc.ProductService.ProductServiceClient>(options =>
{
    options.Address = new Uri("https://localhost:5001");
});

// Istniej¹ce serwisy
builder.Services.AddHttpClient<IOpenAIClient,OpenAIClient>();
builder.Services.AddSingleton<OpenAIClient>();

builder.Services.AddAutoMapper(typeof(OpenFoodFactsProfile));

// Database configuration - supports both Docker and local PostgreSQL

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddAuthorization();
builder.Services.AddControllers();

// Add health checks services
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

// Health check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/alive");

// Configure gRPC services (nowe modu³owe serwisy)
app.MapGrpcService<AuthGrpcService>();
app.MapGrpcService<ProductsGrpcService>();

app.UseCors(policy =>
    policy.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());


app.UseAuthentication();
app.UseAuthorization();

// Mapowanie kontrolerów REST API (interfejs zewnêtrzny)
app.MapControllers();

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Database initialization
using (var scope = app.Services.CreateScope())
{
    try 
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();
        
        // Run migrations
        if (db.Database.GetPendingMigrations().Any())
        {
            await db.Database.MigrateAsync();
        }

        // Initialize roles
        var roleInitService = scope.ServiceProvider.GetRequiredService<IRoleInitializationService>();
        await roleInitService.InitializeRolesAsync();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
    }
}

app.UseHttpsRedirection();


// Zachowanie istniej¹cego AI endpointu dla kompatybilnoœci
app.MapPost("/generate-json", async (OpenAIClient client, List<OpenAIMessage> messages) =>
{
    var result = await client.SendPromptForJsonasync(messages);
    return result is not null ? Results.Ok(result) : Results.BadRequest("Could not parse AI response as JSON.");
});

await DbSeeder.SeedData(app);  

app.Run();

