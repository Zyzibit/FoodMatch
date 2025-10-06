using System.Text;
using System.Text.Json;
using inzynierka.AI.OpenAI;
using inzynierka.AI.OpenAI.Model;
using inzynierka.Auth.Model;
using inzynierka.Auth.Services;
using inzynierka.Data;
using inzynierka.Products.Model;
using inzynierka.Products.OpenFoodFacts.Import;
using inzynierka.Products.OpenFoodFacts.Mappings;
using inzynierka.Products.OpenFoodFacts.OpenFoodFactsDeserializer.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

// New modular imports
using inzynierka.Auth.Contracts;
using inzynierka.Products.Contracts;
using inzynierka.AI.Contracts;
using inzynierka.Auth.Modules;
using inzynierka.Products.Modules;
using inzynierka.AI.Modules;
using inzynierka.EventBus;

// gRPC Services - new modular structure
using inzynierka.Auth.Grpc.Services;
using inzynierka.Products.Grpc.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Event Bus (szyna danych)
builder.Services.AddSingleton<IEventBus, InMemoryEventBus>();

// Kontrakty modu³ów (g³ówne interfejsy komunikacji)
builder.Services.AddScoped<IAuthContract, AuthModule>();
builder.Services.AddScoped<IProductsContract, ProductsModule>();
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
builder.Services.AddSingleton<IOpenFoodFactsDeserializer, OpenFoodFactsDeserializer>();
builder.Services.AddSingleton<OpenAIClient>();
builder.Services.AddSingleton<IProductImporter, ProductImporter>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddAutoMapper(typeof(OpenFoodFactsProfile));
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? throw new InvalidOperationException("Connection string 'DefaultConfiguration' not found.")));

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddAuthorization();
builder.Services.AddControllers();

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

// Configure gRPC services (nowe modu³owe serwisy)
app.MapGrpcService<inzynierka.Auth.Grpc.Services.AuthGrpcService>();
app.MapGrpcService<inzynierka.Products.Grpc.Services.ProductsGrpcService>();

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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureDeleted();  
    db.Database.Migrate();       
}

app.UseHttpsRedirection();

// Zachowanie istniej¹cego endpointu testowego
app.MapGet("/test", async (IProductImporter importer) =>
{
    try {
        Console.WriteLine("importing...");
        importer.ImportAsync("sciezka.jsonl", 1000).Wait();
    }catch (Exception ex) {
        return Results.Problem(ex.Message);
    }
    return Results.Ok("Test");
}).RequireAuthorization();

// Zachowanie istniej¹cego AI endpointu dla kompatybilnoœci
app.MapPost("/generate-json", async (OpenAIClient client, List<OpenAIMessage> messages) =>
{
    var result = await client.SendPromptForJsonasync(messages);
    return result is not null ? Results.Ok(result) : Results.BadRequest("Could not parse AI response as JSON.");
});

await DbSeeder.SeedData(app);  

app.Run();

