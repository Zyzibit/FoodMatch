using inzynierka.Users.Model;
using inzynierka.Receipts.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace inzynierka.Data;

public class DbSeeder
{
    public static async Task SeedData(IApplicationBuilder app)
    {
        // Create a scoped service provider to resolve dependencies
        using var scope = app.ApplicationServices.CreateScope();

        // resolve the logger service
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DbSeeder>>();

        try
        {
            // resolve other dependencies
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Seed Units
            if (!await dbContext.Units.AnyAsync())
            {
                logger.LogInformation("Seeding units...");
                var units = new List<Unit>
                {
                    new Unit { Name = "gram" },
                    new Unit { Name = "kilogram" },
                    new Unit { Name = "mililitr" },
                    new Unit { Name = "litr" },
                    new Unit { Name = "sztuka" },
                    new Unit { Name = "łyżka" },
                    new Unit { Name = "łyżeczka" },
                    new Unit { Name = "szklanka" },
                    new Unit { Name = "opakowanie" },
                    new Unit { Name = "garść" },
                    new Unit { Name = "plasterek" },
                    new Unit { Name = "kostka" }
                };
                
                await dbContext.Units.AddRangeAsync(units);
                await dbContext.SaveChangesAsync();
                logger.LogInformation("Units seeded successfully");
            }

            // Create Admin role if it doesn't exist
            if (!await roleManager.RoleExistsAsync(Roles.Admin))
            {
                logger.LogInformation("Admin role is creating");
                var roleResult = await roleManager
                    .CreateAsync(new IdentityRole(Roles.Admin));

                if (!roleResult.Succeeded)
                {
                    var roleErrors = roleResult.Errors.Select(e => e.Description);
                    logger.LogError($"Failed to create admin role. Errors : {string.Join(",", roleErrors)}");
                }
                else
                {
                    logger.LogInformation("Admin role is created");
                }
            }

            if (!await roleManager.RoleExistsAsync(Roles.User))
            {
                logger.LogInformation("User role is creating");
                var roleResult = await roleManager
                    .CreateAsync(new IdentityRole(Roles.User));

                if (!roleResult.Succeeded)
                {
                    var roleErrors = roleResult.Errors.Select(e => e.Description);
                    logger.LogError($"Failed to create user role. Errors : {string.Join(",", roleErrors)}");
                }
                else
                {
                    logger.LogInformation("User role is created");
                }
            }

            var adminUsers = await userManager.GetUsersInRoleAsync(Roles.Admin);
            if (adminUsers.Count == 0)
            {
                var user = new User
                {
                    Name = "Admin",
                    UserName = "admin@gmail.com",
                    Email = "admin@gmail.com",
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                // Attempt to create admin user
                var createUserResult = await userManager
                    .CreateAsync(user, "Admin@123");

                // Validate user creation
                if (!createUserResult.Succeeded)
                {
                    var errors = createUserResult.Errors.Select(e => e.Description);
                    logger.LogError(
                        $"Failed to create admin user. Errors: {string.Join(", ", errors)}"
                    );
                    return;
                }

                // adding role to user
                var addUserToRoleResult = await userManager
                    .AddToRoleAsync(user, Roles.Admin);

                if (!addUserToRoleResult.Succeeded)
                {
                    var errors = addUserToRoleResult.Errors.Select(e => e.Description);
                    logger.LogError($"Failed to add admin role to user. Errors : {string.Join(",", errors)}");
                }
                else
                {
                    logger.LogInformation("Admin user is created");
                }
            }
        }

        catch (Exception ex)
        {
            logger.LogCritical(ex.Message);
        }
    }
}