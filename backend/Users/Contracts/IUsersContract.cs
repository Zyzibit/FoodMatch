using inzynierka.Users.Contracts.Models;

namespace inzynierka.Users.Contracts;

public interface IUsersContract
{
    Task<UserDto?> GetUserByIdAsync(string userId);
    Task<UserDto?> GetUserByUsernameAsync(string username);
    Task<UserDto?> GetUserByEmailAsync(string email);
    Task<List<UserDto>> GetUsersAsync(int pageNumber = 1, int pageSize = 10);
    Task<bool> UpdateUserProfileAsync(string userId, UpdateUserProfileRequest request);
    Task<bool> DeleteUserAsync(string userId);
    Task<int> GetTotalUsersCountAsync();
    Task <bool> UpdateUserFoodPreferencesAsync(string userId, UpdateFoodPreferencesRequest request);
    Task <FoodPreferencesDto> GetUserFoodPreferencesAsync(string userId);
}

