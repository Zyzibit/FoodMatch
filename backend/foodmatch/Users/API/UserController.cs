using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using inzynierka.Users.Services;
using inzynierka.Users.Requests;
using inzynierka.UserPreferences.Services;
using UpdateFoodPreferencesRequest = inzynierka.UserPreferences.Requests.UpdateFoodPreferencesRequest;

namespace inzynierka.Users.API;

[ApiController]
[Route("api/v1/users")]
public class UserController(
    IUserService userService,
    IUserPreferencesService userPreferencesService,
    ILogger<UserController> logger)
    : ControllerBase
{
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

            var user = await userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting current user profile");
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

            var result = await userService.UpdateUserProfileAsync(userId, request);
            if (!result)
            {
                return BadRequest(new { message = "Failed to update user profile" });
            }

            return Ok(new { message = "User profile updated successfully" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating current user profile");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("{userId}")]
    [Authorize]
    public async Task<IActionResult> GetUser(string userId)
    {
        var user = await userService.GetUserByIdAsync(userId);
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
        var user = await userService.GetUserByUsernameAsync(username);
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
        var user = await userService.GetUserByEmailAsync(email);
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
        var users = await userService.GetUsersAsync(pageNumber, pageSize);
        var totalCount = await userService.GetTotalUsersCountAsync();

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
        var result = await userService.UpdateUserProfileAsync(userId, request);
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
        var result = await userService.DeleteUserAsync(userId);
        if (!result)
        {
            return BadRequest(new { message = "Failed to delete user" });
        }

        return Ok(new { message = "User deleted successfully" });
    }

    [HttpPost("profile-picture")]
    [Authorize]
    public async Task<IActionResult> UploadProfilePicture(IFormFile? file)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file provided" });
            }

            var result = await userService.UpdateProfilePictureAsync(userId, file);
            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new { 
                message = "Profile picture uploaded successfully", 
                profilePictureUrl = result.ProfilePictureUrl 
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error uploading profile picture");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpDelete("profile-picture")]
    [Authorize]
    public async Task<IActionResult> DeleteProfilePicture()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await userService.DeleteProfilePictureAsync(userId);
            if (!result)
            {
                return BadRequest(new { message = "Failed to delete profile picture" });
            }

            return Ok(new { message = "Profile picture deleted successfully" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting profile picture");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
    
    [HttpGet("preferences")]
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
            
            var preferences = await userPreferencesService.GetUserFoodPreferencesAsync(userId);
            return Ok(preferences);
        }
        catch (Exception ex) 
        {
            logger.LogError(ex, "Error getting user food preferences");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
    
    [HttpPut("preferences")]
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

            var result = await userPreferencesService.UpdateUserFoodPreferencesAsync(userId, request);
            if (!result) 
            {
                return BadRequest(new { message = "Failed to update food preferences" });
            }

            var updatedPreferences = await userPreferencesService.GetUserFoodPreferencesAsync(userId);
            return Ok(updatedPreferences);
        }
        catch (Exception ex) 
        {
            logger.LogError(ex, "Error updating user food preferences");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}
