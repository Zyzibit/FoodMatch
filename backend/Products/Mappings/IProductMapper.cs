using inzynierka.Products.Services.Models;
using inzynierka.Products.Model;

namespace inzynierka.Products.Mappings;

public interface IProductMapper
{
    ProductInfo MapToProductInfo(Product product);
    NutritionInfo MapToNutritionInfo(Product product);
    IEnumerable<ProductInfo> MapToProductInfoList(IEnumerable<Product> products);
}

