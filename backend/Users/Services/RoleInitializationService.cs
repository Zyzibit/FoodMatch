using inzynierka.Users.Model;

namespace inzynierka.Users.Services;

public class RoleInitializationService : IRoleInitializationService
{
    private readonly IRoleService _roleService;
    private readonly ILogger<RoleInitializationService> _logger;

    public RoleInitializationService(
        IRoleService roleService,
        ILogger<RoleInitializationService> logger)
    {
        _roleService = roleService;
        _logger = logger;
    }

    public async Task InitializeRolesAsync()
    {
        var roles = new[] { Roles.Admin, Roles.User };

        foreach (var role in roles)
        {
            if (!await _roleService.RoleExistsAsync(role))
            {
                _logger.LogInformation("Creating role: {Role}", role);
                
                var result = await _roleService.CreateRoleAsync(role);
                
                if (result)
                {
                    _logger.LogInformation("Role {Role} created successfully", role);
                }
                else
                {
                    _logger.LogError("Failed to create role {Role}", role);
                    throw new InvalidOperationException($"Failed to create role {role}");
                }
            }
            else
            {
                _logger.LogDebug("Role {Role} already exists", role);
            }
        }
    }
}

