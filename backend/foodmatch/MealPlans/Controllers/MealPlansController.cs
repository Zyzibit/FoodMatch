using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using inzynierka.MealPlans.Requests;
using inzynierka.MealPlans.Services;

namespace inzynierka.MealPlans.Controllers;

[ApiController]
[Route("api/v1/mealplans")]
public class MealPlansController : ControllerBase
{
    private readonly IMealPlanService _mealPlanService;
    private readonly ILogger<MealPlansController> _logger;

    public MealPlansController(IMealPlanService mealPlanService, ILogger<MealPlansController> logger)
    {
        _mealPlanService = mealPlanService;
        _logger = logger;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddMealPlan([FromBody] CreateMealPlanRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _mealPlanService.AddMealPlanAsync(userId, request);
            
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding meal plan");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetMealPlansForDate([FromQuery] DateTime date)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _mealPlanService.GetMealPlansForDateAsync(userId, date);
            
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting meal plans for date");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteMealPlan(int id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var success = await _mealPlanService.DeleteMealPlanAsync(userId, id);
            
            if (!success)
            {
                return NotFound(new { message = "Meal plan not found or you don't have permission to delete it" });
            }

            return Ok(new { message = "Meal plan deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting meal plan");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}
