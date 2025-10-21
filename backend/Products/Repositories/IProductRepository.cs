using inzynierka.Products.Model;
using inzynierka.Products.Model.Tag.CategoryTag;
using inzynierka.Products.Model.Tag.AllergenTag;
using inzynierka.Products.Model.Tag.IngredientTag;

namespace inzynierka.Products.Repositories;

public interface IProductRepository
{
    Task<Product?> GetProductByIdAsync(int productId);
    Task<Product?> GetProductByCodeAsync(string productCode);
    Task<Product?> GetProductWithDetailsAsync(int productId);
    Task<IEnumerable<Product>> GetProductsWithDetailsAsync(int limit, int offset);
    Task<IEnumerable<Product>> GetProductsByIdsAsync(List<int> productIds);
    Task<IEnumerable<Product>> SearchProductsAsync(
        string? searchQuery = null,
        string? brand = null,
        IEnumerable<string>? categories = null,
        IEnumerable<string>? allergens = null,
        IEnumerable<string>? ingredients = null,
        int limit = 50,
        int offset = 0);
    Task<IEnumerable<Product>> GetProductsByCategoryAsync(string categoryName, int limit, int offset);
    Task<int> GetTotalProductsCountAsync();
    Task<int> GetProductsCountByCategoryAsync(string categoryName);
    Task<int> GetSearchResultsCountAsync(
        string? searchQuery = null,
        string? brand = null,
        IEnumerable<string>? categories = null,
        IEnumerable<string>? allergens = null,
        IEnumerable<string>? ingredients = null);
    
    Task<IEnumerable<CategoryTag>> GetAllCategoriesAsync();
    Task<CategoryTag?> GetCategoryByNameAsync(string categoryName);
    
    Task<IEnumerable<AllergenTag>> GetAllAllergensAsync();
    Task<IEnumerable<string>> GetAllergenNamesAsync();
    
    Task<IEnumerable<IngredientTag>> GetAllIngredientsAsync();
    Task<IEnumerable<string>> GetIngredientNamesAsync();
    
    Task<Product> AddProductAsync(Product product);
    Task<Product> UpdateProductAsync(Product product);
    Task DeleteProductAsync(int productId);
    Task<bool> ProductExistsAsync(string productCode);
    
    Task AddProductsRangeAsync(IEnumerable<Product> products);
    Task SaveChangesAsync();
}