using inzynierka.Products.OpenFoodFacts.Import;
using inzynierka.Products.OpenFoodFacts.OpenFoodFactsDeserializer.Services;
using inzynierka.Products.OpenFoodFacts.Repositories;

namespace inzynierka.Products.OpenFoodFacts.Extensions;

public static class OpenFoodFactsServiceExtensions
{
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