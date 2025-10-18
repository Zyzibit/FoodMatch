using inzynierka.Products.Repositories;
using inzynierka.Products.Contracts;
using inzynierka.Products.Modules;
using inzynierka.Products.Services;
using inzynierka.Products.OpenFoodFacts.Extensions;

namespace inzynierka.Products.Extensions;

public static class ProductsServiceExtensions
{

    public static IServiceCollection AddProductsServices(this IServiceCollection services)
    {
        services.AddScoped<IProductRepository, ProductRepository>();
        
        services.AddScoped<IProductsContract, ProductsModule>();
        
        services.AddScoped<IRedisCacheService, RedisCacheService>();
        
        services.AddOpenFoodFactsServices();
        
        return services;
    }
}