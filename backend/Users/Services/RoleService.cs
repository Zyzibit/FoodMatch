using inzynierka.Users.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace inzynierka.Users.Services;

public class RoleService : IRoleService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<RoleService> _logger;

    public RoleService(
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<RoleService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task<IList<string>> GetUserRolesAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} not found", userId);
            return new List<string>();
        }

        return await _userManager.GetRolesAsync(user);
    }

    public async Task<bool> AddUserToRoleAsync(string userId, string roleName)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", userId);
                return false;
            }

            if (!await RoleExistsAsync(roleName))
            {
                _logger.LogWarning("Role {RoleName} does not exist", roleName);
                return false;
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to add user {UserId} to role {RoleName}. Errors: {Errors}",
                    userId, roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                return false;
            }

            _logger.LogInformation("User {UserId} successfully added to role {RoleName}", userId, roleName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding user {UserId} to role {RoleName}", userId, roleName);
            return false;
        }
    }

    public async Task<bool> RemoveUserFromRoleAsync(string userId, string roleName)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", userId);
                return false;
            }

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to remove user {UserId} from role {RoleName}. Errors: {Errors}",
                    userId, roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                return false;
            }

            _logger.LogInformation("User {UserId} successfully removed from role {RoleName}", userId, roleName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing user {UserId} from role {RoleName}", userId, roleName);
            return false;
        }
    }

    public async Task<bool> IsInRoleAsync(string userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} not found", userId);
            return false;
        }

        return await _userManager.IsInRoleAsync(user, roleName);
    }

    public async Task<IList<IdentityRole>> GetAllRolesAsync()
    {
        return await _roleManager.Roles.ToListAsync();
    }

    public async Task<bool> RoleExistsAsync(string roleName)
    {
        return await _roleManager.RoleExistsAsync(roleName);
    }

    public async Task<bool> CreateRoleAsync(string roleName)
    {
        try
        {
            if (await RoleExistsAsync(roleName))
            {
                _logger.LogWarning("Role {RoleName} already exists", roleName);
                return false;
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to create role {RoleName}. Errors: {Errors}",
                    roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                return false;
            }

            _logger.LogInformation("Role {RoleName} created successfully", roleName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role {RoleName}", roleName);
            return false;
        }
    }

    public async Task<bool> DeleteRoleAsync(string roleName)
    {
        try
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                _logger.LogWarning("Role {RoleName} not found", roleName);
                return false;
            }

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to delete role {RoleName}. Errors: {Errors}",
                    roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                return false;
            }

            _logger.LogInformation("Role {RoleName} deleted successfully", roleName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role {RoleName}", roleName);
            return false;
        }
    }
}

