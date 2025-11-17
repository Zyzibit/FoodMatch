using inzynierka.Users.Model;
using inzynierka.Users.Responses;
using inzynierka.Users.Requests;

namespace inzynierka.Users.Services;

public interface IUserService
{
    // Public API methods - return DTOs
    Task<UserDto?> GetUserByIdAsync(string userId);
    Task<UserDto?> GetUserByUsernameAsync(string username);
    Task<UserDto?> GetUserByEmailAsync(string email);
    Task<List<UserDto>> GetUsersAsync(int pageNumber, int pageSize);
    Task<bool> UpdateUserProfileAsync(string userId, UpdateUserProfileRequest request);
    Task<bool> DeleteUserAsync(string userId);
    Task<int> GetTotalUsersCountAsync();
    Task<(bool Success, User? User, string? ErrorMessage)> CreateUserAsync(string username, string email, string password, string role = "User");
    Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    Task<bool> UpdateUserFoodPreferencesAsync(string userId, UpdateFoodPreferencesRequest request);
    Task<FoodPreferencesDto?> GetUserFoodPreferencesAsync(string userId);
    
    // Internal methods - return entities (for use by other services like AuthService)
    Task<User?> GetUserEntityByIdAsync(string userId);
    Task<User?> GetUserEntityByUsernameAsync(string username);
    Task<User?> GetUserEntityByEmailAsync(string email);
}
