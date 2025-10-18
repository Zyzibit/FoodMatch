using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using inzynierka.Products.Contracts;
using inzynierka.Products.Contracts.Models;

namespace inzynierka.Products.API;

[ApiController]
[Route("api/v1/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductsContract _productsModule;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductsContract productsModule, ILogger<ProductsController> logger)
    {
        _productsModule = productsModule;
        _logger = logger;
    }

    [HttpGet("{productId}")]
    public async Task<IActionResult> GetProduct(string productId)
    {
        try
        {
            var result = await _productsModule.GetProductAsync(productId);
            
            if (!result.Success)
            {
                return NotFound(new { message = result.ErrorMessage });
            }

            return Ok(result.Product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product: {ProductId}", productId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }


    [HttpGet("search")]
    public async Task<IActionResult> SearchProducts([FromQuery] ProductSearchRequest request)
    {
        try
        {
            var query = new ProductSearchQuery
            {
                Query = request.Query,
                Categories = request.Categories?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                Allergens = request.Allergens?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                Ingredients = request.Ingredients?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                Brand = request.Brand,
                Limit = request.Limit,
                Offset = request.Offset
            };

            var result = await _productsModule.SearchProductsAsync(query);
            
            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new
            {
                products = result.Products,
                totalCount = result.TotalCount,
                hasMore = result.TotalCount > (query.Offset + query.Limit)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }


    [HttpGet("category/{category}")]
    public async Task<IActionResult> GetProductsByCategory(string category, [FromQuery] int limit = 10, [FromQuery] int offset = 0)
    {
        try
        {
            var result = await _productsModule.GetProductsByCategoryAsync(category, limit, offset);
            
            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new
            {
                products = result.Products,
                totalCount = result.TotalCount,
                hasMore = result.TotalCount > (offset + limit)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products by category: {Category}", category);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }


    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        try
        {
            var categories = await _productsModule.GetCategoriesAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }


    [HttpGet("allergens")]
    public async Task<IActionResult> GetAllergens()
    {
        try
        {
            var allergens = await _productsModule.GetAllergensAsync();
            return Ok(allergens);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting allergens");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }


    [HttpGet("ingredients")]
    public async Task<IActionResult> GetIngredients()
    {
        try
        {
            var ingredients = await _productsModule.GetIngredientsAsync();
            return Ok(ingredients);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ingredients");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }


    [HttpGet("{productId}/nutrition")]
    public async Task<IActionResult> GetNutritionInfo(string productId)
    {
        try
        {
            var result = await _productsModule.GetNutritionInfoAsync(productId);
            
            if (!result.Success)
            {
                return NotFound(new { message = result.ErrorMessage });
            }

            return Ok(result.Nutrition);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting nutrition info for product: {ProductId}", productId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }


    [HttpPost("import")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ImportProducts([FromBody] ProductImportRequest request)
    {
        try
        {
            var result = await _productsModule.ImportProductsAsync(request.FilePath, request.MaxProducts, request.BatchSize);
            
            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new
            {
                success = true,
                importedCount = result.ImportedCount,
                failedCount = result.FailedCount,
                warnings = result.Warnings
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing products");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllProducts([FromQuery] int limit = 50, [FromQuery] int offset = 0)
    {
        try
        {
            if (limit <= 0 || limit > 1000)
            {
                return BadRequest(new { message = "Limit must be between 1 and 1000" });
            }

            if (offset < 0)
            {
                return BadRequest(new { message = "Offset must be greater than or equal to 0" });
            }

            var result = await _productsModule.GetAllProductsAsync(limit, offset);
            
            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new
            {
                products = result.Products,
                totalCount = result.TotalCount,
                hasMore = result.TotalCount > (offset + limit),
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all products");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}


public class ProductSearchRequest
{
    public string? Query { get; set; }
    public string? Categories { get; set; }
    public string? Allergens { get; set; }
    public string? Ingredients { get; set; }
    public string? Brand { get; set; }
    public int Limit { get; set; } = 10;
    public int Offset { get; set; } = 0;
}

public class ProductImportRequest
{
    public string FilePath { get; set; } = string.Empty;
    public int MaxProducts { get; set; } = 100000;
    public int BatchSize { get; set; } = 1000;
}