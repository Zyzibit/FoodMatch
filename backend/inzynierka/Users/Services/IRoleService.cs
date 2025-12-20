using Microsoft.AspNetCore.Identity;

namespace inzynierka.Users.Services;

public interface IRoleService
{
    Task<IList<string>> GetUserRolesAsync(string userId);
    Task<bool> AddUserToRoleAsync(string userId, string roleName);
    Task<bool> RemoveUserFromRoleAsync(string userId, string roleName);
    Task<bool> IsInRoleAsync(string userId, string roleName);
    Task<IList<IdentityRole>> GetAllRolesAsync();
    Task<bool> RoleExistsAsync(string roleName);
    Task<bool> CreateRoleAsync(string roleName);
    Task<bool> DeleteRoleAsync(string roleName);
}

