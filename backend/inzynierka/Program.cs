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
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IEmailService, EmailService>();
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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "http://localhost:5127","http://127.0.0.1:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .WithExposedHeaders("Content-Disposition");
    });
});

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Konfiguracja czasu ważności tokenów resetowania hasła
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromHours(24); // Token ważny przez 24 godziny
});

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

app.UseCors("AllowFrontend");

app.MapHealthChecks("/health");
app.MapHealthChecks("/alive");

var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

var keysPath = Path.Combine(builder.Environment.ContentRootPath, "keys");
if (!Directory.Exists(keysPath))
{
    Directory.CreateDirectory(keysPath);
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

// using (var scope = app.Services.CreateScope())
// {
//     var sp = scope.ServiceProvider;
//     var db = sp.GetRequiredService<AppDbContext>();
//     var roleInit = sp.GetRequiredService<IRoleInitializationService>();
//
//     if (app.Environment.IsDevelopment())
//     {
//         // lokalnie - apply migrations i seed synchronnie
//         await db.Database.MigrateAsync();
//         await roleInit.InitializeRolesAsync();
//         await DbSeeder.SeedData(app);
//     }
//     else
//     {
//         // produkcja - uruchom migracje/seed w tle, aby nie blokować startu serwera
//         _ = Task.Run(async () =>
//         {
//             try
//             {
//                 using var bgScope = app.Services.CreateScope();
//                 var bgSp = bgScope.ServiceProvider;
//                 var bgDb = bgSp.GetRequiredService<AppDbContext>();
//                 var bgRoleInit = bgSp.GetRequiredService<IRoleInitializationService>();
//
//                 await bgDb.Database.MigrateAsync();
//                 await bgRoleInit.InitializeRolesAsync();
//                 await DbSeeder.SeedData(app);
//             }
//             catch (Exception ex)
//             {
//                 var logger = app.Services.GetService<ILogger<Program>>();
//                 logger?.LogError(ex, "Błąd podczas migracji/seed w tle");
//             }
//         });
//     }
// }

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// await DbSeeder.SeedData(app);

app.Run();
