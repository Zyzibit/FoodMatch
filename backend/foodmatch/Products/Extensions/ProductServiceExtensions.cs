using inzynierka.Products.Repositories;
using inzynierka.Products.Services;
using inzynierka.Products.OpenFoodFacts.Extensions;

namespace inzynierka.Products.Extensions;

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