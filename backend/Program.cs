using System.Text;
using inzynierka.AI.OpenAI;
using inzynierka.AI.OpenAI.Services;
using inzynierka.Auth.Repositories;
using inzynierka.Data;
using inzynierka.Products.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using inzynierka.Auth.Services;
using inzynierka.MealPlans.Repositories;
using inzynierka.MealPlans.Services;
using inzynierka.Users.Model;
using inzynierka.Users.Services;
using inzynierka.Recipes.Repositories;
using inzynierka.Recipes.Services;
using inzynierka.ShoppingList.Repositories;
using inzynierka.ShoppingList.Services;
using inzynierka.Units.Repositories;
using inzynierka.Units.Services;
using inzynierka.UserPreferences.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);




// Add Aspire service defaults (observability, health checks, etc.)
builder.AddServiceDefaults();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Aspire PostgreSQL with EF Core
builder.AddNpgsqlDbContext<AppDbContext>("foodmatch");

// Add Aspire Redis
builder.AddRedisClient("redis");

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IRoleInitializationService, RoleInitializationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();

builder.Services.AddProductsServices();

builder.Services.AddHostedService<TokenCleanupService>();

builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();
builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<IUnitRepository, UnitRepository>();
builder.Services.AddScoped<IUnitService, UnitService>();
builder.Services.AddScoped<IPromptConfigService, PromptConfigService>();
builder.Services.AddScoped<IRecipeGeneratorService, RecipeGeneratorService>();

builder.Services.AddScoped<IMealPlanService, MealPlanService>();
builder.Services.AddScoped<IMealPlanRepository, MealPlanRepository>();

builder.Services.AddScoped<IShoppingListService, ShoppingListService>();
builder.Services.AddScoped<IShoppingListRepository, ShoppingListRepository>();

builder.Services.AddUserPreferencesServices();

builder.Services.AddScoped<IAiClient, AiClient>();

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
    .AddRedis(builder.Configuration.GetConnectionString("redis") 
              ?? builder.Configuration.GetConnectionString("Redis") 
              ?? "127.0.0.1:6379");

// Get backend URL from Aspire or fallback to config
var backendUrl = builder.Configuration["services:backend:http:0"] 
                 ?? builder.Configuration["JWT:ValidAudience"] 
                 ?? "http://localhost:5127";

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
                ValidAudience = backendUrl,
                ValidIssuer = backendUrl,
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

// Map Aspire default endpoints (health checks, metrics, etc.)
app.MapDefaultEndpoints();

// Configure CORS for frontend
app.UseCors(policy =>
{
    policy.SetIsOriginAllowed(origin =>
    {
        // Allow any localhost origin
        if (origin.StartsWith("http://localhost:") || origin.StartsWith("https://localhost:"))
        {
            return true;
        }
        
        // Allow 127.0.0.1
        if (origin.StartsWith("http://127.0.0.1:") || origin.StartsWith("https://127.0.0.1:"))
        {
            return true;
        }
        
        // Allow Aspire frontend URLs
        var frontendUrl = builder.Configuration["services:frontend:http:0"] 
                          ?? builder.Configuration["services:frontend:https:0"];
        if (!string.IsNullOrEmpty(frontendUrl) && origin.StartsWith(frontendUrl))
        {
            return true;
        }
        
        return false;
    })
    .AllowCredentials()
    .AllowAnyMethod()
    .AllowAnyHeader();
});

var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

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
