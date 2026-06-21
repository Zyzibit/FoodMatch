using foodmatch.Products.Repositories;
using foodmatch.Products.Services;
using foodmatch.Products.OpenFoodFacts.Extensions;

namespace foodmatch.Products.Extensions;

public static class ProductServiceExtensions
{

    public static IServiceCollection AddProductsServices(this IServiceCollection services)
    {
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductService, ProductService>();
        
        services.AddOpenFoodFactsServices();
        
        return services;
    }
}