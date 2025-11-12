using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using inzynierka.Products.Services;
using inzynierka.Products.Requests;
using inzynierka.Products.Responses;

namespace inzynierka.Products.API;

[ApiController]
[Route("api/v1/products")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productModule;
    private readonly ILogger<ProductController> _logger;

    public ProductController(IProductService productModule, ILogger<ProductController> logger)
    {
        _productModule = productModule;
        _logger = logger;
    }

    [HttpGet("{productId}")]
    public async Task<IActionResult> GetProduct(string productId)
    {
        try
        {
            var result = await _productModule.GetProductAsync(productId);
            
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
            var productSearchDto = new ProductSearchDto
            {
                Query = request.Query,
                Categories = request.Categories?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                Allergens = request.Allergens?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                Ingredients = request.Ingredients?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                Brand = request.Brand,
                Limit = request.Limit,
                Offset = request.Offset
            };

            var result = await _productModule.SearchProductsAsync(productSearchDto);
            
            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new
            {
                products = result.Products,
                totalCount = result.TotalCount,
                hasMore = result.TotalCount > (productSearchDto.Offset + productSearchDto.Limit)
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
            var result = await _productModule.GetProductsByCategoryAsync(category, limit, offset);
            
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
            var categories = await _productModule.GetCategoriesAsync();
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
            var allergens = await _productModule.GetAllergensAsync();
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
            var ingredients = await _productModule.GetIngredientsAsync();
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
            var result = await _productModule.GetNutritionInfoAsync(productId);
            
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
            var result = await _productModule.ImportProductsAsync(request.FilePath);
            
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

            var result = await _productModule.GetAllProductsAsync(limit, offset);
            
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