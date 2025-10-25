using Microsoft.EntityFrameworkCore;
using inzynierka.Data;
using inzynierka.Products.Model;
using inzynierka.Products.Model.Tag.CategoryTag;
using inzynierka.Products.Model.Tag.AllergenTag;
using inzynierka.Products.Model.Tag.IngredientTag;

namespace inzynierka.Products.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProductRepository> _logger;

    public ProductRepository(AppDbContext context, ILogger<ProductRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Product?> GetProductByIdAsync(int productId)
    {
        try
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product by ID: {ProductId}", productId);
            throw;
        }
    }

    public async Task<Product?> GetProductByCodeAsync(string productCode)
    {
        try
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.Code == productCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product by code: {ProductCode}", productCode);
            throw;
        }
    }

    public async Task<Product?> GetProductByNameAsync(string productName)
    {
        try
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.ProductName != null && p.ProductName.ToLower() == productName.ToLower());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product by name: {ProductName}", productName);
            throw;
        }
    }

    public async Task<Product?> GetProductWithDetailsAsync(int productId)
    {
        try
        {
            return await _context.Products
                .Include(p => p.ProductAllergenTags).ThenInclude(pat => pat.AllergenTag)
                .Include(p => p.ProductCategoryTags).ThenInclude(pct => pct.CategoryTag)
                .Include(p => p.ProductIngredientTags).ThenInclude(pit => pit.IngredientTag)
                .Include(p => p.ProductCountryTags).ThenInclude(pct => pct.CountryTag)
                .FirstOrDefaultAsync(p => p.Id == productId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product with details by ID: {ProductId}", productId);
            throw;
        }
    }

    public async Task<IEnumerable<Product>> GetProductsWithDetailsAsync(int limit, int offset)
    {
        try
        {
            return await _context.Products
                .Include(p => p.ProductAllergenTags).ThenInclude(pat => pat.AllergenTag)
                .Include(p => p.ProductCategoryTags).ThenInclude(pct => pct.CategoryTag)
                .Include(p => p.ProductIngredientTags).ThenInclude(pit => pit.IngredientTag)
                .Include(p => p.ProductCountryTags).ThenInclude(pct => pct.CountryTag)
                .OrderBy(p => p.Id)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products with details");
            throw;
        }
    }

    public async Task<IEnumerable<Product>> GetProductsByIdsAsync(List<int> productIds)
    {
        try
        {
            return await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products by IDs: {ProductIds}", string.Join(", ", productIds));
            throw;
        }
    }

    public async Task<IEnumerable<Product>> SearchProductsAsync(
        string? searchQuery = null,
        string? brand = null,
        IEnumerable<string>? categories = null,
        IEnumerable<string>? allergens = null,
        IEnumerable<string>? ingredients = null,
        int limit = 50,
        int offset = 0)
    {
        try
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(p => p.ProductName!.Contains(searchQuery) ||
                                        p.Brands!.Contains(searchQuery));
            }

            if (!string.IsNullOrEmpty(brand))
            {
                query = query.Where(p => p.Brands!.Contains(brand));
            }

            if (categories?.Any() == true)
            {
                query = query.Where(p => p.ProductCategoryTags
                    .Any(pct => categories.Contains(pct.CategoryTag.Name)));
            }

            if (allergens?.Any() == true)
            {
                query = query.Where(p => p.ProductAllergenTags
                    .Any(pat => allergens.Contains(pat.AllergenTag.Name)));
            }

            if (ingredients?.Any() == true)
            {
                query = query.Where(p => p.ProductIngredientTags
                    .Any(pit => ingredients.Contains(pit.IngredientTag.Name)));
            }

            return await query
                .Include(p => p.ProductAllergenTags).ThenInclude(pat => pat.AllergenTag)
                .Include(p => p.ProductCategoryTags).ThenInclude(pct => pct.CategoryTag)
                .Include(p => p.ProductIngredientTags).ThenInclude(pit => pit.IngredientTag)
                .Include(p => p.ProductCountryTags).ThenInclude(pct => pct.CountryTag)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products with query: {SearchQuery}", searchQuery);
            throw;
        }
    }

    public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(string categoryName, int limit, int offset)
    {
        try
        {
            return await _context.Products
                .Where(p => p.ProductCategoryTags.Any(pct => pct.CategoryTag.Name == categoryName))
                .Include(p => p.ProductAllergenTags).ThenInclude(pat => pat.AllergenTag)
                .Include(p => p.ProductCategoryTags).ThenInclude(pct => pct.CategoryTag)
                .Include(p => p.ProductIngredientTags).ThenInclude(pit => pit.IngredientTag)
                .Include(p => p.ProductCountryTags).ThenInclude(pct => pct.CountryTag)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products by category: {CategoryName}", categoryName);
            throw;
        }
    }

    public async Task<int> GetTotalProductsCountAsync()
    {
        try
        {
            return await _context.Products.CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total products count");
            throw;
        }
    }

    public async Task<int> GetProductsCountByCategoryAsync(string categoryName)
    {
        try
        {
            return await _context.Products
                .CountAsync(p => p.ProductCategoryTags.Any(pct => pct.CategoryTag.Name == categoryName));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products count by category: {CategoryName}", categoryName);
            throw;
        }
    }

    public async Task<int> GetSearchResultsCountAsync(
        string? searchQuery = null,
        string? brand = null,
        IEnumerable<string>? categories = null,
        IEnumerable<string>? allergens = null,
        IEnumerable<string>? ingredients = null)
    {
        try
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(p => p.ProductName!.Contains(searchQuery) ||
                                        p.Brands!.Contains(searchQuery));
            }

            if (!string.IsNullOrEmpty(brand))
            {
                query = query.Where(p => p.Brands!.Contains(brand));
            }

            if (categories?.Any() == true)
            {
                query = query.Where(p => p.ProductCategoryTags
                    .Any(pct => categories.Contains(pct.CategoryTag.Name)));
            }

            if (allergens?.Any() == true)
            {
                query = query.Where(p => p.ProductAllergenTags
                    .Any(pat => allergens.Contains(pat.AllergenTag.Name)));
            }

            if (ingredients?.Any() == true)
            {
                query = query.Where(p => p.ProductIngredientTags
                    .Any(pit => ingredients.Contains(pit.IngredientTag.Name)));
            }

            return await query.CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting search results count");
            throw;
        }
    }

    public async Task<IEnumerable<CategoryTag>> GetAllCategoriesAsync()
    {
        try
        {
            return await _context.CategoryTags
                .Include(ct => ct.ProductCategoryTags)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all categories");
            throw;
        }
    }

    public async Task<CategoryTag?> GetCategoryByNameAsync(string categoryName)
    {
        try
        {
            return await _context.CategoryTags
                .FirstOrDefaultAsync(ct => ct.Name == categoryName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category by name: {CategoryName}", categoryName);
            throw;
        }
    }

    public async Task<IEnumerable<AllergenTag>> GetAllAllergensAsync()
    {
        try
        {
            return await _context.AllergenTags.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all allergens");
            throw;
        }
    }

    public async Task<IEnumerable<string>> GetAllergenNamesAsync()
    {
        try
        {
            return await _context.AllergenTags
                .Select(at => at.Name)
                .Distinct()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting allergen names");
            throw;
        }
    }

    public async Task<IEnumerable<IngredientTag>> GetAllIngredientsAsync()
    {
        try
        {
            return await _context.IngredientTags.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all ingredients");
            throw;
        }
    }

    public async Task<IEnumerable<string>> GetIngredientNamesAsync()
    {
        try
        {
            return await _context.IngredientTags
                .Select(it => it.Name)
                .Distinct()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ingredient names");
            throw;
        }
    }

    public async Task<Product> AddProductAsync(Product product)
    {
        try
        {
            var result = await _context.Products.AddAsync(product);
            return result.Entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding product: {ProductCode}", product.Code);
            throw;
        }
    }

    public async Task<Product> UpdateProductAsync(Product product)
    {
        try
        {
            var result = _context.Products.Update(product);
            return result.Entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product: {ProductId}", product.Id);
            throw;
        }
    }

    public async Task DeleteProductAsync(int productId)
    {
        try
        {
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                _context.Products.Remove(product);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product: {ProductId}", productId);
            throw;
        }
    }

    public async Task<bool> ProductExistsAsync(string productCode)
    {
        try
        {
            return await _context.Products.AnyAsync(p => p.Code == productCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if product exists: {ProductCode}", productCode);
            throw;
        }
    }

    public async Task AddProductsRangeAsync(IEnumerable<Product> products)
    {
        try
        {
            await _context.Products.AddRangeAsync(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding products range");
            throw;
        }
    }

    public async Task SaveChangesAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving changes");
            throw;
        }
    }
}