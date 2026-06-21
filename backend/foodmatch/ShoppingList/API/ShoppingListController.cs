using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using inzynierka.ShoppingList.Requests;
using inzynierka.ShoppingList.Services;

namespace inzynierka.ShoppingList.API;

[ApiController]
[Route("api/v1/shopping-list")]
[Authorize]
public class ShoppingListController : ControllerBase
{
    private readonly IShoppingListService _shoppingListService;
    private readonly ILogger<ShoppingListController> _logger;

    public ShoppingListController(IShoppingListService shoppingListService, ILogger<ShoppingListController> logger)
    {
        _shoppingListService = shoppingListService;
        _logger = logger;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetShoppingList()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _shoppingListService.GetShoppingListAsync(userId);
            
            if (result == null)
            {
                return NotFound(new { message = "Shopping list not found" });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting shopping list");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
    
    [HttpPost("items")]
    public async Task<IActionResult> AddProduct([FromBody] AddProductToShoppingListRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _shoppingListService.AddProductToShoppingListAsync(userId, request);
            
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding product to shopping list");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
    
    [HttpPut("items/{itemId}")]
    public async Task<IActionResult> UpdateItem(int itemId, [FromBody] UpdateShoppingListItemRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _shoppingListService.UpdateShoppingListItemAsync(userId, itemId, request);
            
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating shopping list item");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
    
    [HttpDelete("items/{itemId}")]
    public async Task<IActionResult> RemoveProduct(int itemId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _shoppingListService.RemoveProductFromShoppingListAsync(userId, itemId);
            
            if (!result)
            {
                return NotFound(new { message = "Item not found or unauthorized" });
            }

            return Ok(new { message = "Product removed from shopping list" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing product from shopping list");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
    
    [HttpDelete]
    public async Task<IActionResult> ClearShoppingList()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _shoppingListService.ClearShoppingListAsync(userId);
            
            if (!result)
            {
                return NotFound(new { message = "Shopping list not found" });
            }

            return Ok(new { message = "Shopping list cleared" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing shopping list");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}

