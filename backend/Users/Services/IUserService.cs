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
    Task<User?> AddUserAsync(string username, string email, string password, string name, string role = "User");
}
