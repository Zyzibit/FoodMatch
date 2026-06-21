using foodmatch.Products.Requests;
using foodmatch.Products.Responses;
using foodmatch.Products.Model;
using foodmatch.Recipes.Model.RecipeModel;

namespace foodmatch.Products.Services;

public interface IProductService
{
    Task<ProductResult> GetProductAsync(string productId);
    Task<ProductSearchResult> SearchProductsAsync(ProductSearchDto dto);
    Task<ProductSearchResult> GetAllProductsAsync(int limit = 50, int offset = 0);
    Task<ProductCategoryResult> GetProductsByCategoryAsync(string category, int limit = 10, int offset = 0);
    Task<ProductImportResult> ImportProductsAsync(string filePath, CancellationToken cancellationToken = default);
    Task<List<ProductCategory>> GetCategoriesAsync();
    Task<List<string>> GetAllergensAsync();
    Task<List<string>> GetIngredientsAsync();
    Task<ProductNutritionResult> GetNutritionInfoAsync(string productId);
    Task<IEnumerable<ProductDto>> GetProductsByIdsAsync(IEnumerable<int> ids);
    Task<Product> CreateAiGeneratedProductAsync(GeneratedRecipeIngredient ingredient);
    string GetProductDisplayName(ProductDto product);
    bool IsProductMatchingIngredient(ProductDto product, ProductDto ingredient);
    ProductDto? FindMatchingProduct(ProductDto ingredient, List<ProductDto> availableProducts);
    List<ProductDto> GetMatchingProducts(List<ProductDto> availableProducts, List<ProductDto> ingredients);
}