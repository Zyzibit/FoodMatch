using inzynierka.Products.Contracts;
using inzynierka.Products.Contracts.Models;
using inzynierka.Data;
using inzynierka.EventBus;
using inzynierka.Products.EventBus.Events;
using inzynierka.Products.OpenFoodFacts.Import;
using Microsoft.EntityFrameworkCore;

namespace inzynierka.Products.Modules;

/// <summary>
/// Implementacja kontraktu produktów - modu³ Products
/// </summary>
public class ProductsModule : IProductsContract
{
    private readonly AppDbContext _context;
    private readonly IProductImporter _productImporter;
    private readonly IEventBus _eventBus;
    private readonly ILogger<ProductsModule> _logger;

    public ProductsModule(
        AppDbContext context,
        IProductImporter productImporter,
        IEventBus eventBus,
        ILogger<ProductsModule> logger)
    {
        _context = context;
        _productImporter = productImporter;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<ProductResult> GetProductAsync(string productId)
    {
        try
        {
            var product = await _context.Products
                .Include(p => p.ProductAllergenTags).ThenInclude(pat => pat.AllergenTag)
                .Include(p => p.ProductCategoryTags).ThenInclude(pct => pct.CategoryTag)
                .Include(p => p.ProductIngredientTags).ThenInclude(pit => pit.IngredientTag)
                .Include(p => p.ProductCountryTags).ThenInclude(pct => pct.CountryTag)
                .FirstOrDefaultAsync(p => p.Id.ToString() == productId);

            if (product == null)
            {
                return new ProductResult
                {
                    Success = false,
                    ErrorMessage = "Product not found"
                };
            }

            // Publikacja zdarzenia wyœwietlenia produktu
            await _eventBus.PublishAsync(new ProductViewedEvent
            {
                UserId = "", // To mo¿na uzupe³niæ gdy bêdzie dostêpny context u¿ytkownika
                ProductId = productId,
                ViewTime = DateTime.UtcNow
            });

            return new ProductResult
            {
                Success = true,
                Product = new ProductInfo
                {
                    Id = product.Id.ToString(),
                    Name = product.ProductName ?? "",
                    Brand = product.Brands ?? "",
                    Barcode = product.Code ?? "",
                    ImageUrl = product.ImageUrl ?? "",
                    Categories = product.ProductCategoryTags.Select(pct => pct.CategoryTag.Name).ToList(),
                    Ingredients = product.ProductIngredientTags.Select(pit => pit.IngredientTag.Name).ToList(),
                    Allergens = product.ProductAllergenTags.Select(pat => pat.AllergenTag.Name).ToList(),
                    Countries = product.ProductCountryTags.Select(pct => pct.CountryTag.Name).ToList(),
                    NutritionGrade = product.NutritionGrade,
                    EcoScoreGrade = product.EcoScoreGrade
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product with ID: {ProductId}", productId);
            return new ProductResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<ProductSearchResult> SearchProductsAsync(ProductSearchQuery query)
    {
        try
        {
            var dbQuery = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(query.Query))
            {
                dbQuery = dbQuery.Where(p => p.ProductName!.Contains(query.Query) ||
                                            p.Brands!.Contains(query.Query));
            }

            if (!string.IsNullOrEmpty(query.Brand))
            {
                dbQuery = dbQuery.Where(p => p.Brands!.Contains(query.Brand));
            }

            if (query.Categories?.Any() == true)
            {
                dbQuery = dbQuery.Where(p => p.ProductCategoryTags
                    .Any(pct => query.Categories.Contains(pct.CategoryTag.Name)));
            }

            if (query.Allergens?.Any() == true)
            {
                dbQuery = dbQuery.Where(p => p.ProductAllergenTags
                    .Any(pat => query.Allergens.Contains(pat.AllergenTag.Name)));
            }

            if (query.Ingredients?.Any() == true)
            {
                dbQuery = dbQuery.Where(p => p.ProductIngredientTags
                    .Any(pit => query.Ingredients.Contains(pit.IngredientTag.Name)));
            }

            var totalCount = await dbQuery.CountAsync();

            var products = await dbQuery
                .Include(p => p.ProductAllergenTags).ThenInclude(pat => pat.AllergenTag)
                .Include(p => p.ProductCategoryTags).ThenInclude(pct => pct.CategoryTag)
                .Include(p => p.ProductIngredientTags).ThenInclude(pit => pit.IngredientTag)
                .Include(p => p.ProductCountryTags).ThenInclude(pct => pct.CountryTag)
                .Skip(query.Offset)
                .Take(query.Limit)
                .ToListAsync();

            var productInfos = products.Select(p => new ProductInfo
            {
                Id = p.Id.ToString(),
                Name = p.ProductName ?? "",
                Brand = p.Brands ?? "",
                Barcode = p.Code ?? "",
                ImageUrl = p.ImageUrl ?? "",
                Categories = p.ProductCategoryTags.Select(pct => pct.CategoryTag.Name).ToList(),
                Ingredients = p.ProductIngredientTags.Select(pit => pit.IngredientTag.Name).ToList(),
                Allergens = p.ProductAllergenTags.Select(pat => pat.AllergenTag.Name).ToList(),
                Countries = p.ProductCountryTags.Select(pct => pct.CountryTag.Name).ToList(),
                NutritionGrade = p.NutritionGrade,
                EcoScoreGrade = p.EcoScoreGrade
            }).ToList();

            // Publikacja zdarzenia wyszukiwania
            await _eventBus.PublishAsync(new ProductSearchedEvent
            {
                UserId = "",
                Query = query.Query ?? "",
                ResultsCount = totalCount
            });

            return new ProductSearchResult
            {
                Success = true,
                Products = productInfos,
                TotalCount = totalCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products with query: {Query}", query.Query);
            return new ProductSearchResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<ProductCategoryResult> GetProductsByCategoryAsync(string category, int limit = 10, int offset = 0)
    {
        try
        {
            var query = _context.Products
                .Where(p => p.ProductCategoryTags.Any(pct => pct.CategoryTag.Name == category));

            var totalCount = await query.CountAsync();

            var products = await query
                .Include(p => p.ProductAllergenTags).ThenInclude(pat => pat.AllergenTag)
                .Include(p => p.ProductCategoryTags).ThenInclude(pct => pct.CategoryTag)
                .Include(p => p.ProductIngredientTags).ThenInclude(pit => pit.IngredientTag)
                .Include(p => p.ProductCountryTags).ThenInclude(pct => pct.CountryTag)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            var productInfos = products.Select(p => new ProductInfo
            {
                Id = p.Id.ToString(),
                Name = p.ProductName ?? "",
                Brand = p.Brands ?? "",
                Barcode = p.Code ?? "",
                ImageUrl = p.ImageUrl ?? "",
                Categories = p.ProductCategoryTags.Select(pct => pct.CategoryTag.Name).ToList(),
                Ingredients = p.ProductIngredientTags.Select(pit => pit.IngredientTag.Name).ToList(),
                Allergens = p.ProductAllergenTags.Select(pat => pat.AllergenTag.Name).ToList(),
                Countries = p.ProductCountryTags.Select(pct => pct.CountryTag.Name).ToList(),
                NutritionGrade = p.NutritionGrade,
                EcoScoreGrade = p.EcoScoreGrade
            }).ToList();

            // Publikacja zdarzenia dostêpu do kategorii
            await _eventBus.PublishAsync(new ProductCategoryAccessedEvent
            {
                CategoryName = category,
                ProductCount = totalCount,
                UserId = ""
            });

            return new ProductCategoryResult
            {
                Success = true,
                Products = productInfos,
                TotalCount = totalCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products by category: {Category}", category);
            return new ProductCategoryResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<ProductImportResult> ImportProductsAsync(string filePath, int batchSize = 1000)
    {
        try
        {
            var startTime = DateTime.UtcNow;
            
            await _productImporter.ImportAsync(filePath, batchSize);
            
            var duration = DateTime.UtcNow - startTime;

            // Publikacja zdarzenia importu
            await _eventBus.PublishAsync(new ProductImportedEvent
            {
                ImportedCount = batchSize,
                FilePath = filePath,
                Duration = duration
            });

            return new ProductImportResult
            {
                Success = true,
                ImportedCount = batchSize,
                FailedCount = 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing products from: {FilePath}", filePath);
            return new ProductImportResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                ImportedCount = 0,
                FailedCount = 0
            };
        }
    }

    public async Task<List<ProductCategory>> GetCategoriesAsync()
    {
        try
        {
            var categories = await _context.CategoryTags
                .Select(ct => new ProductCategory
                {
                    Id = ct.Id.ToString(),
                    Name = ct.Name,
                    ProductCount = ct.ProductCategoryTags.Count
                })
                .ToListAsync();

            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories");
            return new List<ProductCategory>();
        }
    }

    public async Task<List<string>> GetAllergensAsync()
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
            _logger.LogError(ex, "Error getting allergens");
            return new List<string>();
        }
    }

    public async Task<List<string>> GetIngredientsAsync()
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
            _logger.LogError(ex, "Error getting ingredients");
            return new List<string>();
        }
    }

    public async Task<ProductNutritionResult> GetNutritionInfoAsync(string productId)
    {
        try
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id.ToString() == productId);

            if (product == null)
            {
                return new ProductNutritionResult
                {
                    Success = false,
                    ErrorMessage = "Product not found"
                };
            }

            // Publikacja zdarzenia dostêpu do informacji ¿ywieniowych
            await _eventBus.PublishAsync(new NutritionInfoAccessedEvent
            {
                ProductId = productId,
                UserId = "",
                AccessTime = DateTime.UtcNow
            });

            var nutritionInfo = new NutritionInfo
            {
                Energy = null,
                Fat = null,
                Carbohydrates = null,
                Proteins = null,
            };

            return new ProductNutritionResult
            {
                Success = true,
                Nutrition = nutritionInfo
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting nutrition info for product: {ProductId}", productId);
            return new ProductNutritionResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}