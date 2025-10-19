using inzynierka.Data;
using inzynierka.Auth.Model;
using Microsoft.EntityFrameworkCore;

namespace inzynierka.Users.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<UserService> _logger;

    public UserService(AppDbContext dbContext, ILogger<UserService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<User?> GetUserByIdAsync(string userId)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        return user;
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == username);
        return user;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        return user;
    }

    public async Task<List<User>> GetUsersAsync(int pageNumber, int pageSize)
    {
        return await _dbContext.Users
            .OrderBy(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<bool> UpdateUserProfileAsync(string userId, string? name, string? email)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} not found", userId);
            return false;
        }

        if (!string.IsNullOrEmpty(name))
            user.Name = name;

        if (!string.IsNullOrEmpty(email))
            user.Email = email;

        user.UpdatedAt = DateTime.UtcNow;

        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} not found", userId);
            return false;
        }

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetTotalUsersCountAsync()
    {
        return await _dbContext.Users.CountAsync();
    }

}
