using inzynierka.Products.Responses;
using inzynierka.Products.Model;

namespace inzynierka.Products.Mappings;

public interface IProductMapper
{
    ProductDto MapToProductInfo(Product product);
    NutritionInfoDto MapToNutritionInfo(Product product);
    IEnumerable<ProductDto> MapToProductInfoList(IEnumerable<Product> products);
}
