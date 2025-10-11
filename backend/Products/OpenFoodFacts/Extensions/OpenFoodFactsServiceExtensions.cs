using inzynierka.Products.OpenFoodFacts.Import;
using inzynierka.Products.OpenFoodFacts.OpenFoodFactsDeserializer.Services;
using inzynierka.Products.OpenFoodFacts.Repositories;

namespace inzynierka.Products.OpenFoodFacts.Extensions;

/// <summary>
/// Rozszerzenia do rejestracji serwisów OpenFoodFacts
/// </summary>
public static class OpenFoodFactsServiceExtensions
{
    /// <summary>
    /// Rejestruje wszystkie serwisy OpenFoodFacts w kontenerze DI
    /// </summary>
    /// <param name="services">Kolekcja serwisów</param>
    /// <returns>Kolekcja serwisów z zarejestrowanymi serwisami OpenFoodFacts</returns>
    public static IServiceCollection AddOpenFoodFactsServices(this IServiceCollection services)
    {
        // Repository layer
        services.AddScoped<IOpenFoodFactsRepository, OpenFoodFactsRepository>();
        
        // Cache services
        
        // Services
        services.AddScoped<IProductImporter, ProductImporter>();
        services.AddSingleton<IOpenFoodFactsDeserializer, inzynierka.Products.OpenFoodFacts.OpenFoodFactsDeserializer.Services.OpenFoodFactsDeserializer>();
        
        return services;
    }
}