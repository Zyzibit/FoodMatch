using inzynierka.Products.Dto;
using inzynierka.Products.Responses;
using inzynierka.Products.Repositories;
using inzynierka.Products.Model;
using inzynierka.Products.Extensions;
using inzynierka.Recipes.Model.RecipeModel;

namespace inzynierka.Products.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IProductImportService _productImportService;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository productRepository,
        IProductImportService productImportService,
        ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _productImportService = productImportService;
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


            return new ProductResult
            {
                Success = true,
                Product = product.ToProductDto()
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

    public async Task<ProductSearchResult> SearchProductsAsync(ProductSearchDto dto
    )
    {
        try
        {
            var totalCount = await _productRepository.GetSearchResultsCountAsync(
                searchQuery: dto.Query,
                brand: dto.Brand,
                categories: dto.Categories,
                allergens: dto.Allergens,
                ingredients: dto.Ingredients);

            var products = await _productRepository.SearchProductsAsync(
                searchQuery: dto.Query,
                brand: dto.Brand,
                categories: dto.Categories,
                allergens: dto.Allergens,
                ingredients: dto.Ingredients,
                limit: dto.Limit,
                offset: dto.Offset);

            var productInfos = products.ToProductDtoList().ToList();



            return new ProductSearchResult
            {
                Success = true,
                Products = productInfos,
                TotalCount = totalCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products with query: {Query}", dto.Query);
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

            var productInfos = products.ToProductDtoList().ToList();

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

            var productInfos = products.ToProductDtoList().ToList();

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
            await _productImportService.ImportProductsAsync(filePath);

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
            

            var nutritionInfo = product.ToNutritionInfoDto();

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


    public async Task<IEnumerable<ProductDto>> GetProductsByIdsAsync(IEnumerable<int> ids)
    {
        var idList = ids.Where(id => id > 0).Distinct().ToList();
        if (!idList.Any()) return Enumerable.Empty<ProductDto>();

        try
        {
            var products = await _productRepository.GetProductsByIdsAsync(idList);
            
            var productInfos = products.ToProductDtoList().ToList();

            return productInfos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania produktów po Id: {Ids}", string.Join(", ", idList));
            return Enumerable.Empty<ProductDto>();
        }
    }
    public async Task<Product> CreateAiGeneratedProductAsync(GeneratedRecipeIngredient ingredient)
    {
        if (string.IsNullOrWhiteSpace(ingredient.Name))
        {
            throw new ArgumentException("Ingredient name cannot be null or empty", nameof(ingredient));
        }

        try
        {
            var existingProduct = await _productRepository.GetProductByNameAsync(ingredient.Name.Trim());
            
            if (existingProduct != null)
            {
                return existingProduct;
            }

            decimal normalizedQuantity = 100m; 
            if (ingredient.NormalizedQuantityInGrams.HasValue && ingredient.NormalizedQuantityInGrams.Value > 0)
            {
                normalizedQuantity = ingredient.NormalizedQuantityInGrams.Value;
            }
            else if (ingredient.Unit.ToLowerInvariant() == "g" && ingredient.Quantity > 0)
            {
                normalizedQuantity = ingredient.Quantity;
            }
            else
            {
                _logger.LogWarning(
                    "Ingredient '{IngredientName}' does not have a valid normalized quantity. " +
                    "Using default of 100g for nutritional calculations.", 
                    ingredient.Name);
            }
            var scaleFactor = 100m / normalizedQuantity;
            
            var aiProduct = new Product
            {
                Code = $"AI-GENERATED-{Guid.NewGuid()}",
                ProductName = ingredient.Name.Trim(),
                Language = "pl",
                estimatedCalories = ingredient.EstimatedCalories * scaleFactor,
                estimatedProteins = ingredient.EstimatedProteins * scaleFactor,
                estimatedCarbohydrates = ingredient.EstimatedCarbohydrates * scaleFactor,
                estimatedFats = ingredient.EstimatedFats * scaleFactor,
                LastUpdated = DateTime.UtcNow,
                Source = ProductSource.AI
            };
            var createdProduct = await _productRepository.AddProductAsync(aiProduct);
            await _productRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Created new AI-generated product: {ProductName} with ID: {ProductId}. " +
                "Normalized from {OriginalGrams}g (CaloriesGoal: {OriginalCalories}) to 100g (CaloriesGoal: {NormalizedCalories})", 
                ingredient.Name, createdProduct.Id, normalizedQuantity, ingredient.EstimatedCalories, aiProduct.estimatedCalories);

            return createdProduct;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating AI-generated product: {ProductName}", ingredient.Name);
            throw new InvalidOperationException($"Failed to create product for ingredient '{ingredient.Name}'", ex);
        }
    }
    public string GetProductDisplayName(ProductDto product)
    {
        return !string.IsNullOrWhiteSpace(product.Name)
            ? product.Name
            : (!string.IsNullOrWhiteSpace(product.Brand)
                ? product.Brand
                : $"Product {product.Id}");
    }

    public bool IsProductMatchingIngredient(ProductDto product, ProductDto ingredient)
    {
        var ingredientNameLower = ingredient.Name.ToLowerInvariant();

        if (!string.IsNullOrWhiteSpace(product.Name))
        {
            var productNameLower = product.Name.ToLowerInvariant();
            if (ingredientNameLower.Contains(productNameLower) ||
                productNameLower.Contains(ingredientNameLower))
            {
                return true;
            }
        }

        if (!string.IsNullOrWhiteSpace(product.Brand))
        {
            var brandLower = product.Brand.ToLowerInvariant();
            if (ingredientNameLower.Contains(brandLower) ||
                brandLower.Contains(ingredientNameLower))
            {
                return true;
            }
        }

        return false;
    }

    public ProductDto? FindMatchingProduct(ProductDto ingredient, List<ProductDto> availableProducts)
    {
        var matchingProduct = availableProducts.FirstOrDefault(p =>
            IsProductMatchingIngredient(p, ingredient));
        return matchingProduct;
    }

    public List<ProductDto> GetMatchingProducts(List<ProductDto> availableProducts, List<ProductDto> ingredients)
    {
        var matchingProducts = availableProducts
            .Where(p => ingredients.Any(ingredient =>
                IsProductMatchingIngredient(p, ingredient)))
            .ToList();

        if (matchingProducts.Count < availableProducts.Count)
        {
            var unusedProducts = availableProducts.Except(matchingProducts)
                .Select(p => GetProductDisplayName(p));
            _logger.LogInformation(
                "Matched {MatchedCount}/{TotalCount} products. Unused: {UnusedProducts}",
                matchingProducts.Count, availableProducts.Count, string.Join(", ", unusedProducts));
        }

        return matchingProducts;
    }
}
