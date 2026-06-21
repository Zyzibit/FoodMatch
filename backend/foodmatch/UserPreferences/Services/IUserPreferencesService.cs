using foodmatch.UserPreferences.Responses;
using foodmatch.UserPreferences.Requests;

namespace foodmatch.UserPreferences.Services;

public interface IUserPreferencesService
{
    Task<FoodPreferencesDto?> GetUserFoodPreferencesAsync(string userId);
    Task<bool> UpdateUserFoodPreferencesAsync(string userId, UpdateFoodPreferencesRequest request);
}

