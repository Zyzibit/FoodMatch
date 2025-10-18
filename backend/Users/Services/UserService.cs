using inzynierka.Auth.Model;
using inzynierka.Users.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace inzynierka.Users.Services;

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<UserService> _logger;

    public UserService(UserManager<User> userManager, ILogger<UserService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<UserProfile?> GetUserByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user != null ? MapToUserProfile(user) : null;
    }

    public async Task<UserProfile?> GetUserByUsernameAsync(string username)
    {
        var user = await _userManager.FindByNameAsync(username);
        return user != null ? MapToUserProfile(user) : null;
    }

    public async Task<UserProfile?> GetUserByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user != null ? MapToUserProfile(user) : null;
    }

    public async Task<List<UserProfile>> GetUsersAsync(int pageNumber, int pageSize)
    {
        var users = await _userManager.Users
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return users.Select(MapToUserProfile).ToList();
    }

    public async Task<bool> UpdateUserProfileAsync(string userId, string? name, string? email)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} not found", userId);
            return false;
        }

        if (!string.IsNullOrEmpty(name))
        {
            user.Name = name;
        }

        if (!string.IsNullOrEmpty(email) && email != user.Email)
        {
            var setEmailResult = await _userManager.SetEmailAsync(user, email);
            if (!setEmailResult.Succeeded)
            {
                _logger.LogError("Failed to update email for user {UserId}: {Errors}", 
                    userId, string.Join(", ", setEmailResult.Errors.Select(e => e.Description)));
                return false;
            }
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            _logger.LogError("Failed to update user {UserId}: {Errors}", 
                userId, string.Join(", ", result.Errors.Select(e => e.Description)));
            return false;
        }

        return true;
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} not found", userId);
            return false;
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            _logger.LogError("Failed to delete user {UserId}: {Errors}", 
                userId, string.Join(", ", result.Errors.Select(e => e.Description)));
            return false;
        }

        return true;
    }

    public async Task<int> GetTotalUsersCountAsync()
    {
        return await _userManager.Users.CountAsync();
    }

    private static UserProfile MapToUserProfile(User user)
    {
        return new UserProfile
        {
            Id = user.Id,
            Name = user.Name,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            CreatedAt = DateTime.UtcNow, 
            UpdatedAt = null
        };
    }
}

