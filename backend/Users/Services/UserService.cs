using inzynierka.Data;
using inzynierka.Users.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace inzynierka.Users.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<UserService> _logger;
    private readonly UserManager<User> _userManager;

    public UserService(AppDbContext dbContext, ILogger<UserService> logger, UserManager<User> userManager)
    {
        _dbContext = dbContext;
        _logger = logger;
        _userManager = userManager;
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

    public async Task<User?> AddUserAsync(string username, string email, string password, string name, string role = "User")
    {
        var user = new User
        {
            UserName = username,
            Email = email,
            Name = name,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, password);
        
        if (!result.Succeeded)
        {
            _logger.LogWarning("Failed to create user {Username}. Errors: {Errors}", 
                username, string.Join(", ", result.Errors.Select(e => e.Description)));
            return null;
        }

        var roleResult = await _userManager.AddToRoleAsync(user, role);
        
        if (!roleResult.Succeeded)
        {
            _logger.LogWarning("User {Username} created but failed to assign role {Role}. Errors: {Errors}", 
                username, role, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
        }

        _logger.LogInformation("User {Username} created successfully with role {Role}", username, role);
        return user;
    }
}
