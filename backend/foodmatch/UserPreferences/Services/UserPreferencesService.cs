using inzynierka.Users.Model;
using inzynierka.UserPreferences.Responses;
using inzynierka.UserPreferences.Requests;
using inzynierka.UserPreferences.Extensions;
using Microsoft.AspNetCore.Identity;

namespace inzynierka.UserPreferences.Services;

public class UserPreferencesService : IUserPreferencesService
{
    private readonly ILogger<UserPreferencesService> _logger;
    private readonly UserManager<User> _userManager;

    public UserPreferencesService(
        ILogger<UserPreferencesService> logger, 
        UserManager<User> userManager)
    {
        _logger = logger;
        _userManager = userManager;
    }

    public async Task<FoodPreferencesDto?> GetUserFoodPreferencesAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user?.FoodPreferences?.ToDto();
    }
    
    public async Task<bool> UpdateUserFoodPreferencesAsync(
        string userId,
        UpdateFoodPreferencesRequest request)
    {
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Cannot update food preferences: userId is null or empty.");
            return false;
        }

        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", userId);
                return false;
            }

            user.FoodPreferences.UpdateFrom(request);

            user.UpdatedAt = DateTime.UtcNow;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to update food preferences for user {UserId}. Errors: {Errors}", userId, errors);
                return false;
            }

            _logger.LogInformation("Food preferences updated for user {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating food preferences for user {UserId}", userId);
            return false;
        }
    }
}

