using inzynierka.Products.Repositories;
using inzynierka.Products.Contracts;
using inzynierka.Products.Modules;
using inzynierka.Products.OpenFoodFacts.Extensions;

namespace inzynierka.Products.Extensions;

public static class ProductServiceExtensions
{

    public static IServiceCollection AddProductsServices(this IServiceCollection services)
    {
        services.AddScoped<IProductRepository, ProductRepository>();
        
        services.AddScoped<IProductContract, ProductModule>();
        
        services.AddOpenFoodFactsServices();
        
        return services;
    }
}