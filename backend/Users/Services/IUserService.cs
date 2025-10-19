using inzynierka.Users.Contracts.Models;
using inzynierka.Users.Model;

namespace inzynierka.Users.Services;

public interface IUserService
{
    Task<User?> GetUserByIdAsync(string userId);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByEmailAsync(string email);
    Task<List<User>> GetUsersAsync(int pageNumber, int pageSize);
    Task<bool> UpdateUserProfileAsync(string userId, string? name, string? email);
    Task<bool> DeleteUserAsync(string userId);
    Task<int> GetTotalUsersCountAsync();
    Task<(bool Success, User? User, string? ErrorMessage)> CreateUserAsync(string username, string email, string password, string role = "User");
    Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    Task<bool> UpdateUserFoodPreferencesAsync(string userId, FoodPreferences foodPreferences) ;

    Task<FoodPreferences?> GetUserFoodPreferencesAsync(string userId);
}
