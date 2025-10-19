using inzynierka.Products.Contracts;
using inzynierka.Products.Contracts.Models;
using inzynierka.Products.Repositories;
using inzynierka.EventBus;
using inzynierka.Products.EventBus.Events;
using inzynierka.Products.OpenFoodFacts.Import;

namespace inzynierka.Products.Modules;

public class ProductsModule : IProductsContract
{
    private readonly IProductRepository _productRepository;
    private readonly IProductImporter _productImporter;
    private readonly IEventBus _eventBus;
    private readonly ILogger<ProductsModule> _logger;

    public ProductsModule(
        IProductRepository productRepository,
        IProductImporter productImporter,
        IEventBus eventBus,
        ILogger<ProductsModule> logger)
    {
        _productRepository = productRepository;
        _productImporter = productImporter;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<ProductResult> GetProductAsync(string productId)
    {
        try
        {
            if (!int.TryParse(productId, out var id))
            {
                return new ProductResult
                {
                    Success = false,
                    ErrorMessage = "Invalid product ID format"
                };
            }

            var product = await _productRepository.GetProductWithDetailsAsync(id);

            if (product == null)
            {
                return new ProductResult
                {
                    Success = false,
                    ErrorMessage = "Product not found"
                };
            }

            await _eventBus.PublishAsync(new ProductViewedEvent
            {
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
                    Nutrition = new NutritionInfo {
                        Carbohydrates = product.Carbohydrates100g,
                        Proteins = product.Proteins100g,
                        Fat = product.Fat100g,
                        Energy = product.Energy100g
                    },
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
            var totalCount = await _productRepository.GetSearchResultsCountAsync(
                searchQuery: query.Query,
                brand: query.Brand,
                categories: query.Categories,
                allergens: query.Allergens,
                ingredients: query.Ingredients);

            var products = await _productRepository.SearchProductsAsync(
                searchQuery: query.Query,
                brand: query.Brand,
                categories: query.Categories,
                allergens: query.Allergens,
                ingredients: query.Ingredients,
                limit: query.Limit,
                offset: query.Offset);

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

    public async Task<ProductSearchResult> GetAllProductsAsync(int limit = 50, int offset = 0)
    {
        try
        {
            var totalCount = await _productRepository.GetTotalProductsCountAsync();
            var products = await _productRepository.GetProductsWithDetailsAsync(limit, offset);

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

            await _eventBus.PublishAsync(new ProductSearchedEvent
            {
                UserId = "",
                Query = "all_products",
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
            _logger.LogError(ex, "Error getting all products");
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
            var totalCount = await _productRepository.GetProductsCountByCategoryAsync(category);
            var products = await _productRepository.GetProductsByCategoryAsync(category, limit, offset);

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

    public async Task<ProductImportResult> ImportProductsAsync(string filePath)
    {
        try
        {
            var startTime = DateTime.UtcNow;
            
            await _productImporter.ImportJsonlAsync(filePath);
            
            var duration = DateTime.UtcNow - startTime;

            await _eventBus.PublishAsync(new ProductImportedEvent
            {
                FilePath = filePath,
                Duration = duration
            });

            return new ProductImportResult
            {
                Success = true,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing products from: {FilePath}", filePath);
            return new ProductImportResult
            {
                Success = false,
                ErrorMessage = ex.Message,
            };
        }
    }

    public async Task<List<ProductCategory>> GetCategoriesAsync()
    {
        try
        {
            var categories = await _productRepository.GetAllCategoriesAsync();

            return categories.Select(ct => new ProductCategory
            {
                Id = ct.Id.ToString(),
                Name = ct.Name,
                ProductCount = ct.ProductCategoryTags.Count
            }).ToList();
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
            return (await _productRepository.GetAllergenNamesAsync()).ToList();
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
            return (await _productRepository.GetIngredientNamesAsync()).ToList();
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
            if (!int.TryParse(productId, out var id))
            {
                return new ProductNutritionResult
                {
                    Success = false,
                    ErrorMessage = "Invalid product ID format"
                };
            }

            var product = await _productRepository.GetProductByIdAsync(id);

            if (product == null)
            {
                return new ProductNutritionResult
                {
                    Success = false,
                    ErrorMessage = "Product not found"
                };
            }

            await _eventBus.PublishAsync(new NutritionInfoAccessedEvent
            {
                ProductId = productId,
                UserId = "",
                AccessTime = DateTime.UtcNow
            });

            var nutritionInfo = new NutritionInfo
            {
                Energy = product.Energy100g,
                Fat = product.Fat100g,
                Carbohydrates = product.Carbohydrates100g,
                Proteins = product.Proteins100g,
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