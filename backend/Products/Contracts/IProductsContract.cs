using inzynierka.Products.Contracts.Models;

namespace inzynierka.Products.Contracts;

/// <summary>
/// Kontrakt dla modu³u produktów - definiuje interfejs komunikacji
/// </summary>
public interface IProductsContract
{
    Task<ProductResult> GetProductAsync(string productId);
    Task<ProductSearchResult> SearchProductsAsync(ProductSearchQuery query);
    Task<ProductCategoryResult> GetProductsByCategoryAsync(string category, int limit = 10, int offset = 0);
    Task<ProductImportResult> ImportProductsAsync(string filePath, int batchSize = 1000);
    Task<List<ProductCategory>> GetCategoriesAsync();
    Task<List<string>> GetAllergensAsync();
    Task<List<string>> GetIngredientsAsync();
    Task<ProductNutritionResult> GetNutritionInfoAsync(string productId);
}