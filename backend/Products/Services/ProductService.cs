using inzynierka.AI.Contracts.Models;
using inzynierka.Products.Responses;
using inzynierka.Products.Repositories;
using inzynierka.Products.Model;
using inzynierka.Products.OpenFoodFacts.Import;
using inzynierka.Products.Mappings;

namespace inzynierka.Products.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IProductImporter _productImporter;
    private readonly ILogger<ProductService> _logger;
    private readonly IProductMapper _productMapper;

    public ProductService(
        IProductRepository productRepository,
        IProductImporter productImporter,
        ILogger<ProductService> logger,
        IProductMapper productMapper)
    {
        _productRepository = productRepository;
        _productImporter = productImporter;
        _logger = logger;
        _productMapper = productMapper;
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


            return new ProductResult
            {
                Success = true,
                Product = _productMapper.MapToProductInfo(product)
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

    public async Task<ProductSearchResult> SearchProductsAsync(ProductSearchQuery query
    )
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

            var productInfos = _productMapper.MapToProductInfoList(products).ToList();



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

            var productInfos = _productMapper.MapToProductInfoList(products).ToList();

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

            var productInfos = _productMapper.MapToProductInfoList(products).ToList();

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
            await _productImporter.ImportJsonlAsync(filePath);

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
            

            var nutritionInfo = _productMapper.MapToNutritionInfo(product);

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


    public async Task<IEnumerable<ProductInfo>> GetProductsByIdsAsync(IEnumerable<int> ids)
    {
        if (ids == null) return Enumerable.Empty<ProductInfo>();

        var idList = ids.Where(id => id > 0).Distinct().ToList();
        if (!idList.Any()) return Enumerable.Empty<ProductInfo>();

        try
        {
            var products = await _productRepository.GetProductsByIdsAsync(idList);
            if (products == null) return Enumerable.Empty<ProductInfo>();

            var productInfos = _productMapper.MapToProductInfoList(products).ToList();

            return productInfos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania produktów po Id: {Ids}", string.Join(", ", idList));
            return Enumerable.Empty<ProductInfo>();
        }
    }


    public async Task<ProductResult> AddAiProductAsync(GeneratedRecipeIngredient ingredient)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ingredient.Name))
            {
                return new ProductResult
                {
                    Success = false,
                    ErrorMessage = "Product name cannot be empty"
                };
            }

            var existingProduct = await _productRepository.GetProductByNameAsync(ingredient.Name.Trim());

            
            
            if (existingProduct != null)
            {
                _logger.LogInformation("Product with name '{ProductName}' already exists with ID: {ProductId}",
                    ingredient, existingProduct.Id);
                
                return new ProductResult
                {
                    Success = true,
                    Product = _productMapper.MapToProductInfo(existingProduct)
                };
            }

            var aiProduct = new Product
            {
                Code = $"AI-GENERATED-{Guid.NewGuid()}",
                ProductName = ingredient.Name.Trim(),
                IsAiGenerated = true,
                Language = "pl",
                estimatedCalories = ingredient.EstimatedCalories,
                estimatedProteins = ingredient.EstimatedProteins,
                estimatedCarbohydrates = ingredient.EstimatedCarbohydrates,
                estimatedFats = ingredient.EstimatedFats,
                LastUpdated = DateTime.UtcNow
            };

            var createdProduct = await _productRepository.AddProductAsync(aiProduct);
            await _productRepository.SaveChangesAsync();

            _logger.LogInformation("Created new AI-generated product: {ProductName} with ID: {ProductId}", ingredient,
                createdProduct.Id);

            return new ProductResult
            {
                Success = true,
                Product = _productMapper.MapToProductInfo(createdProduct)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating AI-generated product: {ProductName}", ingredient);
            return new ProductResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
