using inzynierka.Users.Contracts;
using inzynierka.Users.Contracts.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace inzynierka.Users.API;

[ApiController]
[Route("api/v1/users")]
public class UsersController : ControllerBase
{
    private readonly IUsersContract _usersContract;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUsersContract usersContract, ILogger<UsersController> logger)
    {
        _usersContract = usersContract;
        _logger = logger;
    }
    
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUserProfile()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var user = await _usersContract.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user profile");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
    
    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> UpdateCurrentUserProfile([FromBody] UpdateUserProfileRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _usersContract.UpdateUserProfileAsync(userId, request);
            if (!result)
            {
                return BadRequest(new { message = "Failed to update user profile" });
            }

            return Ok(new { message = "User profile updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating current user profile");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("{userId}")]
    [Authorize]
    public async Task<IActionResult> GetUser(string userId)
    {
        var user = await _usersContract.GetUserByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        return Ok(user);
    }

    [HttpGet("username/{username}")]
    [Authorize]
    public async Task<IActionResult> GetUserByUsername(string username)
    {
        var user = await _usersContract.GetUserByUsernameAsync(username);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        return Ok(user);
    }

    [HttpGet("email/{email}")]
    [Authorize]
    public async Task<IActionResult> GetUserByEmail(string email)
    {
        var user = await _usersContract.GetUserByEmailAsync(email);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        return Ok(user);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var users = await _usersContract.GetUsersAsync(pageNumber, pageSize);
        var totalCount = await _usersContract.GetTotalUsersCountAsync();

        return Ok(new
        {
            users,
            totalCount,
            pageNumber,
            pageSize,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }
    
    [HttpPut("{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpdateUserProfileRequest request)
    {
        var result = await _usersContract.UpdateUserProfileAsync(userId, request);
        if (!result)
        {
            return BadRequest(new { message = "Failed to update user profile" });
        }

        return Ok(new { message = "User profile updated successfully" });
    }

    [HttpDelete("{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var result = await _usersContract.DeleteUserAsync(userId);
        if (!result)
        {
            return BadRequest(new { message = "Failed to delete user" });
        }

        return Ok(new { message = "User deleted successfully" });
    }
    
    [HttpGet("preferences")]
    [Authorize]
    public async Task<IActionResult> GetUserFoodPreferences()
    {
        try {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) {
                return Unauthorized(new { message = "Invalid token" });
            }
            var preferences = await _usersContract.GetUserFoodPreferencesAsync(userId);
            return Ok(preferences);
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error getting user food preferences");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
    
    [HttpPost("preferences")]
    [Authorize]
    public async Task<IActionResult> UpdateUserFoodPreferences([FromBody] UpdateFoodPreferencesRequest request)
    {
        try {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _usersContract.UpdateUserFoodPreferencesAsync(userId, request);
            if (!result) {
                return BadRequest(new { message = "Failed to update food preferences" });
            }

            return Ok(new { message = "Food preferences updated successfully" });
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error updating user food preferences");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}
