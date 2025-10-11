using inzynierka.Products.Repositories;
using inzynierka.Products.Contracts;
using inzynierka.Products.Modules;
using inzynierka.Products.Services;
using inzynierka.Products.OpenFoodFacts.Extensions;

namespace inzynierka.Products.Extensions;

/// <summary>
/// Rozszerzenia do rejestracji serwisów modu³u Products
/// </summary>
public static class ProductsServiceExtensions
{
    /// <summary>
    /// Rejestruje wszystkie serwisy modu³u Products w kontenerze DI
    /// </summary>
    /// <param name="services">Kolekcja serwisów</param>
    /// <returns>Kolekcja serwisów z zarejestrowanymi serwisami Products</returns>
    public static IServiceCollection AddProductsServices(this IServiceCollection services)
    {
        // Repository layer
        services.AddScoped<IProductRepository, ProductRepository>();
        
        // Business logic layer (Modules)
        services.AddScoped<IProductsContract, ProductsModule>();
        
        // Services
        services.AddScoped<IRedisCacheService, RedisCacheService>();
        
        // OpenFoodFacts services (includes repository, importer, deserializer)
        services.AddOpenFoodFactsServices();
        
        return services;
    }
}