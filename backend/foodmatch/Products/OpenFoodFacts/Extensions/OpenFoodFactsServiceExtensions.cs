using foodmatch.Products.OpenFoodFacts.Services;
using foodmatch.Products.OpenFoodFacts.Repositories;
using foodmatch.Products.OpenFoodFacts.Adapters;
using foodmatch.Products.Services;
using foodmatch.Products.Repositories;

namespace foodmatch.Products.OpenFoodFacts.Extensions;

public static class OpenFoodFactsServiceExtensions
{
    public static IServiceCollection AddOpenFoodFactsServices(this IServiceCollection services)
    {
        services.AddScoped<OpenFoodFactsRepository>();
        services.AddScoped<IProductBulkRepository>(sp => sp.GetRequiredService<OpenFoodFactsRepository>());
        services.AddScoped<IProductImporter, ProductImporter>();
        services.AddScoped<IProductImportService, OpenFoodFactsImportServiceAdapter>();
        
        return services;
    }
}
