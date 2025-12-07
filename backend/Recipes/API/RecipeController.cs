using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using inzynierka.Recipes.Requests;
using inzynierka.Recipes.Services;

namespace inzynierka.Recipes.API;

[ApiController]
[Route("api/v1/recipes")]
public class RecipeController : ControllerBase
{
    private readonly IRecipeService _recipeService;
    private readonly ILogger<RecipeController> _logger;

    public RecipeController(IRecipeService recipeService, ILogger<RecipeController> logger)
    {
        _recipeService = recipeService;
        _logger = logger;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateRecipe([FromBody] CreateRecipeRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _recipeService.CreateRecipeAsync(userId, request);
            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new { success = true, recipeId = result.RecipeId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating recipe");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
    
    [HttpPost("generate-preview")]
    [Authorize]
    public async Task<IActionResult> GenerateRecipePreview([FromBody] GenerateRecipeRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _recipeService.GenerateRecipePreviewAsync(userId, request);
            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new { 
                success = true, 
                recipe = result.Recipe,
                message = "Recipe preview generated successfully" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recipe preview");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
    
    [HttpPost("save-generated")]
    [Authorize]
    public async Task<IActionResult> SaveGeneratedRecipe([FromBody] SaveGeneratedRecipeRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _recipeService.SaveGeneratedRecipeAsync(userId, request);
            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new { 
                success = true, 
                recipeId = result.RecipeId,
                message = "Recipe saved successfully" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving generated recipe");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }


    [HttpGet]
    public async Task<IActionResult> GetAllRecipes([FromQuery] int limit = 50, [FromQuery] int offset = 0)
    {
        try
        {
            var result = await _recipeService.Recipes(limit, offset);
            if (!result.Success)
            {
                return BadRequest(new { message = "Failed to get recipes" });
            }

            return Ok(new {
                recipes = result.Recipes,
                totalCount = result.TotalCount,
                limit,
                offset,
                hasMore = result.TotalCount > (offset + limit)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all recipes");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("community")]
    public async Task<IActionResult> GetCommunityRecipes([FromQuery] int limit = 50, [FromQuery] int offset = 0)
    {
        try
        {
            var result = await _recipeService.GetPublicRecipesAsync(limit, offset);
            if (!result.Success)
            {
                return BadRequest(new { message = "Failed to get community recipes" });
            }

            return Ok(new {
                recipes = result.Recipes,
                totalCount = result.TotalCount,
                limit,
                offset,
                hasMore = result.TotalCount > (offset + limit)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting community recipes");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMyRecipes([FromQuery] int limit = 50, [FromQuery] int offset = 0)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _recipeService.GetUserRecipesAsync(userId, limit, offset);
            if (!result.Success)
            {
                return BadRequest(new { message = "Failed to get user recipes" });
            }

            return Ok(new {
                recipes = result.Recipes,
                totalCount = result.TotalCount,
                limit,
                offset,
                hasMore = result.TotalCount > (offset + limit)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user recipes");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetRecipe(int id)
    {
        try
        {
            var recipe = await _recipeService.GetRecipeAsync(id);
            if (recipe == null)
            {
                return NotFound(new { message = "Recipe not found" });
            }

            return Ok(recipe);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recipe {Id}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPost("{id:int}/copy")]
    [Authorize]
    public async Task<IActionResult> CopyRecipeToAccount(int id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _recipeService.CopyRecipeToUserAsync(userId, id);
            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new { 
                success = true, 
                recipeId = result.RecipeId,
                message = "Recipe copied to your account successfully" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error copying recipe {Id}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
    
    [HttpPatch("{id:int}/share")]
    [Authorize]
    public async Task<IActionResult> ShareRecipe(int id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _recipeService.ShareRecipeAsync(userId, id);
            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new { 
                success = true, 
                message = "Recipe shared successfully" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sharing recipe {Id}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
    
    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteRecipe(int id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _recipeService.DeleteRecipeAsync(userId, id);
            if (!result)
            {
                return NotFound(new { message = "Recipe not found or you don't have permission to delete it" });
            }

            return Ok(new { 
                success = true, 
                message = "Recipe deleted successfully" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting recipe {Id}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}

