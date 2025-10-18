using inzynierka.Users.Model;

namespace inzynierka.Users.Services;

public interface IUserService
{
    Task<UserProfile?> GetUserByIdAsync(string userId);
    Task<UserProfile?> GetUserByUsernameAsync(string username);
    Task<UserProfile?> GetUserByEmailAsync(string email);
    Task<List<UserProfile>> GetUsersAsync(int pageNumber, int pageSize);
    Task<bool> UpdateUserProfileAsync(string userId, string? name, string? email);
    Task<bool> DeleteUserAsync(string userId);
    Task<int> GetTotalUsersCountAsync();
}
