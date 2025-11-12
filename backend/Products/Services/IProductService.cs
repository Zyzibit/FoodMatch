using inzynierka.Products.Dto;
using inzynierka.Products.Model;
using inzynierka.Products.Responses;
using inzynierka.Receipts.Model.Recipe;

namespace inzynierka.Products.Services;

public interface IProductService
{
    Task<ProductResult> GetProductAsync(string productId);
    Task<ProductSearchResult> SearchProductsAsync(ProductSearchDto dto);
    Task<ProductSearchResult> GetAllProductsAsync(int limit = 50, int offset = 0);
    Task<ProductCategoryResult> GetProductsByCategoryAsync(string category, int limit = 10, int offset = 0);
    Task<ProductImportResult> ImportProductsAsync(string filePath);
    Task<List<ProductCategory>> GetCategoriesAsync();
    Task<List<string>> GetAllergensAsync();
    Task<List<string>> GetIngredientsAsync();
    Task<ProductNutritionResult> GetNutritionInfoAsync(string productId);
    Task<IEnumerable<ProductDto>> GetProductsByIdsAsync(IEnumerable<int> ids);
    Task<ProductResult> AddAiProductAsync(GeneratedRecipeIngredient ingredient);
    Task<Product> CreateAiGeneratedProductAsync(GeneratedRecipeIngredient ingredient);
}