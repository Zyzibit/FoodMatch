using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using inzynierka.UserPreferences.Services;
using inzynierka.UserPreferences.Requests;

namespace inzynierka.UserPreferences.API;

[ApiController]
[Route("api/v1/user-preferences")]
public class UserPreferencesController : ControllerBase
{
    private readonly IUserPreferencesService _userPreferencesService;
    private readonly ILogger<UserPreferencesController> _logger;

    public UserPreferencesController(
        IUserPreferencesService userPreferencesService,
        ILogger<UserPreferencesController> logger)
    {
        _userPreferencesService = userPreferencesService;
        _logger = logger;
    }
    
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetUserFoodPreferences()
    {
        try 
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) 
            {
                return Unauthorized(new { message = "Invalid token" });
            }
            
            var preferences = await _userPreferencesService.GetUserFoodPreferencesAsync(userId);
            return Ok(preferences);
        }
        catch (Exception ex) 
        {
            _logger.LogError(ex, "Error getting user food preferences");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
    
    [HttpPut]
    [Authorize]
    public async Task<IActionResult> UpdateUserFoodPreferences([FromBody] UpdateFoodPreferencesRequest request)
    {
        try 
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) 
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _userPreferencesService.UpdateUserFoodPreferencesAsync(userId, request);
            if (!result) 
            {
                return BadRequest(new { message = "Failed to update food preferences" });
            }

            var updatedPreferences = await _userPreferencesService.GetUserFoodPreferencesAsync(userId);
            return Ok(updatedPreferences);
        }
        catch (Exception ex) 
        {
            _logger.LogError(ex, "Error updating user food preferences");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}

