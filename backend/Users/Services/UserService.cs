using inzynierka.Users.Model;
using inzynierka.Users.Responses;
using inzynierka.Users.Requests;
using inzynierka.Users.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace inzynierka.Users.Services;

public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IRoleService _roleService;

    public UserService(
        ILogger<UserService> logger, 
        UserManager<User> userManager,
        IRoleService roleService)
    {
        _logger = logger;
        _userManager = userManager;
        _roleService = roleService;
    }

    public async Task<UserDto?> GetUserByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user?.ToDto();
    }

    public async Task<UserDto?> GetUserByUsernameAsync(string username)
    {
        var user = await _userManager.FindByNameAsync(username);
        return user?.ToDto();
        
    }
    
    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user?.ToDto();
    }
    
    // Internal methods - return entities
    public async Task<User?> GetUserEntityByIdAsync(string userId)
    {
        return await _userManager.FindByIdAsync(userId);
    }

    public async Task<User?> GetUserEntityByUsernameAsync(string username)
    {
        return await _userManager.FindByNameAsync(username);
    }

    public async Task<User?> GetUserEntityByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<List<UserDto>> GetUsersAsync(int pageNumber, int pageSize)
    {
        var users = await _userManager.Users
            .OrderBy(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return users.Select(u => u.ToDto()).ToList();
    }

    public async Task<bool> UpdateUserProfileAsync(string userId, UpdateUserProfileRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} not found", userId);
            return false;
        }

        var hasChanges = false;

        if (!string.IsNullOrEmpty(request.Name) && user.Name != request.Name)
        {
            user.Name = request.Name;
            hasChanges = true;
        }

        if (!string.IsNullOrEmpty(request.Email) && user.Email != request.Email)
        {
            var setEmailResult = await _userManager.SetEmailAsync(user, request.Email);
            if (!setEmailResult.Succeeded)
            {
                _logger.LogError("Failed to set email for user {UserId}. Errors: {Errors}",
                    userId, string.Join(", ", setEmailResult.Errors.Select(e => e.Description)));
                return false;
            }
            hasChanges = true;
        }

        if (hasChanges)
        {
            user.UpdatedAt = DateTime.UtcNow;
            var result = await _userManager.UpdateAsync(user);
            
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to update user {UserId}. Errors: {Errors}", 
                    userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                return false;
            }
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
            _logger.LogError("Failed to delete user {UserId}. Errors: {Errors}", 
                userId, string.Join(", ", result.Errors.Select(e => e.Description)));
            return false;
        }

        _logger.LogInformation("User {UserId} deleted successfully", userId);
        return true;
    }

    public async Task<int> GetTotalUsersCountAsync()
    {
        return await _userManager.Users.CountAsync();
    }

    public async Task<User?> AddUserAsync(string username, string email, string password, string name, string role = "User")
    {
        var result = await CreateUserAsync(username, email, password, role);
        
        if (result.Success && result.User != null)
        {
            if (!string.IsNullOrEmpty(name))
            {
                result.User.Name = name;
                await _userManager.UpdateAsync(result.User);
            }
            return result.User;
        }

        return null;
    }

    private async Task<bool> UserExistsByUsernameAsync(string username)
    {
        return await _userManager.FindByNameAsync(username) != null;
    }

    private async Task<bool> UserExistsByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email) != null;
    }

    public async Task<(bool Success, User? User, string? ErrorMessage)> CreateUserAsync(
        string username, 
        string email, 
        string password, 
        string role = "User")
    {
        try
        {
            if (await UserExistsByUsernameAsync(username))
            {
                _logger.LogWarning("Attempt to create user with existing username: {Username}", username);
                return (false, null, "User with this username already exists");
            }

            if (await UserExistsByEmailAsync(email))
            {
                _logger.LogWarning("Attempt to create user with existing email: {Email}", email);
                return (false, null, "User with this email already exists");
            }

            var user = new User
            {
                UserName = username,
                Email = email,
                SecurityStamp = Guid.NewGuid().ToString(),
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                var errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Failed to create user {Username}. Errors: {Errors}", username, errorMessage);
                return (false, null, errorMessage);
            }
            var roleAssigned = await EnsureRoleAssignedAsync(user, role);
            if (!roleAssigned)
            {
                _logger.LogWarning("User {Username} created but role assignment had issues", username);
            }

            _logger.LogInformation("User {Username} created successfully with role {Role}", username, role);
            return (true, user, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user {Username}", username);
            return (false, null, "An error occurred while creating the user");
        }
    }

    public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", userId);
                return false;
            }

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to change password for user {UserId}. Errors: {Errors}",
                    userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                return false;
            }

            _logger.LogInformation("Password changed successfully for user {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {UserId}", userId);
            return false;
        }
    }

    public async Task<FoodPreferencesDto?> GetUserFoodPreferencesAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user?.FoodPreferences?.ToDto();
    }
    
    public async Task<bool> UpdateUserFoodPreferencesAsync(
        string userId,
        UpdateFoodPreferencesRequest request)
    {
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Cannot update food preferences: userId is null or empty.");
            return false;
        }

        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", userId);
                return false;
            }

            user.FoodPreferences.IsVegan = request.IsVegan;
            user.FoodPreferences.IsVegetarian = request.IsVegetarian;
            user.FoodPreferences.HasGlutenIntolerance = request.HasGlutenIntolerance;
            user.FoodPreferences.HasLactoseIntolerance = request.HasLactoseIntolerance;
            user.FoodPreferences.HasNutAllergy = request.HasNutAllergy;
            user.FoodPreferences.DailyProteinGoal = request.DailyProteinGoal;
            user.FoodPreferences.DailyCarbohydrateGoal = request.DailyCarbohydrateGoal;
            user.FoodPreferences.DailyFatGoal = request.DailyFatGoal;
            user.FoodPreferences.DailyCalorieGoal = request.DailyCalorieGoal;

            user.UpdatedAt = DateTime.UtcNow;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to update food preferences for user {UserId}. Errors: {Errors}", userId, errors);
                return false;
            }

            _logger.LogInformation("Food preferences updated for user {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating food preferences for user {UserId}", userId);
            return false;
        }
    }

    private async Task<bool> EnsureRoleAssignedAsync(User user, string role)
    {
        try
        {
            if (!await _roleService.RoleExistsAsync(role))
            {
                _logger.LogWarning("Role {Role} does not exist. Creating it now.", role);
                var roleCreated = await _roleService.CreateRoleAsync(role);
                if (!roleCreated)
                {
                    _logger.LogError("Failed to create role {Role}", role);
                    return false;
                }
            }

            // Assign role to user
            var roleAssigned = await _roleService.AddUserToRoleAsync(user.Id, role);
            if (!roleAssigned)
            {
                _logger.LogError("Failed to assign role {Role} to user {UserId}", role, user.Id);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring role {Role} for user {UserId}", role, user.Id);
            return false;
        }
    }
}
