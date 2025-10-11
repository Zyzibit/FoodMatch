using inzynierka.Auth.Model;
using Microsoft.AspNetCore.Identity;

namespace inzynierka.Auth.Services;

public interface IRoleInitializationService
{
    Task InitializeRolesAsync();
}

public class RoleInitializationService : IRoleInitializationService
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<RoleInitializationService> _logger;

    public RoleInitializationService(
        RoleManager<IdentityRole> roleManager,
        ILogger<RoleInitializationService> logger)
    {
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task InitializeRolesAsync()
    {
        var roles = new[] { Roles.Admin, Roles.User };

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                _logger.LogInformation("Creating role: {Role}", role);
                
                var result = await _roleManager.CreateAsync(new IdentityRole(role));
                
                if (result.Succeeded)
                {
                    _logger.LogInformation("Role {Role} created successfully", role);
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to create role {Role}. Errors: {Errors}", role, errors);
                    throw new InvalidOperationException($"Failed to create role {role}: {errors}");
                }
            }
            else
            {
                _logger.LogDebug("Role {Role} already exists", role);
            }
        }
    }
}