using inzynierka.Products.OpenFoodFacts.Import;
using inzynierka.Products.OpenFoodFacts.OpenFoodFactsDeserializer.Services;
using inzynierka.Products.OpenFoodFacts.Repositories;
using inzynierka.Products.OpenFoodFacts.Adapters;
using inzynierka.Products.Services;
using inzynierka.Products.Repositories;

namespace inzynierka.Products.OpenFoodFacts.Extensions;

public static class OpenFoodFactsServiceExtensions
{
    public static IServiceCollection AddOpenFoodFactsServices(this IServiceCollection services)
    {
        services.AddScoped<OpenFoodFactsRepository>();
        services.AddScoped<IOpenFoodFactsRepository>(sp => sp.GetRequiredService<OpenFoodFactsRepository>());
        services.AddScoped<IProductBulkRepository>(sp => sp.GetRequiredService<OpenFoodFactsRepository>());
        services.AddScoped<IProductImporter, ProductImporter>();
        services.AddSingleton<IOpenFoodFactsDeserializer, inzynierka.Products.OpenFoodFacts.OpenFoodFactsDeserializer.Services.OpenFoodFactsDeserializer>();
        services.AddScoped<IProductImportService, OpenFoodFactsImportServiceAdapter>();
        
        return services;
    }
}
