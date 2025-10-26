using inzynierka.Products.Repositories;
using inzynierka.Products.Contracts;
using inzynierka.Products.Modules;
using inzynierka.Products.OpenFoodFacts.Extensions;
using inzynierka.Products.Mappings;

namespace inzynierka.Products.Extensions;

public static class ProductServiceExtensions
{

    public static IServiceCollection AddProductsServices(this IServiceCollection services)
    {
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductMapper, ProductMapper>();
        services.AddScoped<IProductContract, ProductModule>();
        
        services.AddOpenFoodFactsServices();
        
        return services;
    }
}