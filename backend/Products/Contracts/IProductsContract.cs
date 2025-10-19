using inzynierka.Products.Contracts.Models;

namespace inzynierka.Products.Contracts;

public interface IProductsContract
{
    Task<ProductResult> GetProductAsync(string productId);
    Task<ProductSearchResult> SearchProductsAsync(ProductSearchQuery query);
    Task<ProductSearchResult> GetAllProductsAsync(int limit = 50, int offset = 0);
    Task<ProductCategoryResult> GetProductsByCategoryAsync(string category, int limit = 10, int offset = 0);
    Task<ProductImportResult> ImportProductsAsync(string filePath, int maxProducts = 100000, int batchSize = 1000);
    Task<List<ProductCategory>> GetCategoriesAsync();
    Task<List<string>> GetAllergensAsync();
    Task<List<string>> GetIngredientsAsync();
    Task<ProductNutritionResult> GetNutritionInfoAsync(string productId);
}