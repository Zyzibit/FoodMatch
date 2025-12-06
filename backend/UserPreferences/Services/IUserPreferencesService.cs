using inzynierka.UserPreferences.Responses;
using inzynierka.UserPreferences.Requests;

namespace inzynierka.UserPreferences.Services;

public interface IUserPreferencesService
{
    Task<FoodPreferencesDto?> GetUserFoodPreferencesAsync(string userId);
    Task<bool> UpdateUserFoodPreferencesAsync(string userId, UpdateFoodPreferencesRequest request);
}

