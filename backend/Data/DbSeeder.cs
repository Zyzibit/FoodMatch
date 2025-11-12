using inzynierka.Users.Model;
using inzynierka.Receipts.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace inzynierka.Data;

public class DbSeeder
{
    public static async Task SeedData(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();

        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DbSeeder>>();

        try
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (!await dbContext.Units.AnyAsync())
            {
                logger.LogInformation("Seeding units...");
                var units = new List<Unit>
                {
                    new Unit 
                    { 
                        Name = "gram", 
                        Description = "Unit of mass in the metric system",
                        PromptDescription = "Use for solid ingredients weight (e.g., flour, sugar, meat). 1000 grams = 1 kilogram"
                    },
                    new Unit()
                    {
                        Name = "ząbek",
                        Description = "Clove - small segment of a bulb (typically garlic)",
                        PromptDescription = "Use for garlic cloves (e.g., 2 cloves of garlic)"
                    },
                    new Unit 
                    { 
                        Name = "kilogram", 
                        Description = "Unit of mass equal to 1000 grams",
                        PromptDescription = "Use for heavier solid ingredients (e.g., large pieces of meat, bulk vegetables)"
                    },
                    new Unit 
                    { 
                        Name = "mililitr", 
                        Description = "Unit of volume in the metric system",
                        PromptDescription = "Use for liquid ingredients (e.g., water, milk, oil). 1000 milliliters = 1 liter"
                    },
                    new Unit 
                    { 
                        Name = "litr", 
                        Description = "Unit of volume equal to 1000 milliliters",
                        PromptDescription = "Use for larger amounts of liquid ingredients"
                    },
                    new Unit 
                    { 
                        Name = "sztuka", 
                        Description = "Counting unit for individual items",
                        PromptDescription = "Use for countable items (e.g., 2 eggs, 3 apples, 1 onion)"
                    },
                    new Unit 
                    { 
                        Name = "łyżka", 
                        Description = "Tablespoon - approximately 15ml",
                        PromptDescription = "Use for small amounts of ingredients (e.g., 2 tablespoons of oil, 1 tablespoon of honey)"
                    },
                        new Unit 
                    { 
                        Name = "łyżeczka", 
                        Description = "Teaspoon - approximately 5ml",
                        PromptDescription = "Use for very small amounts, typically spices and seasonings (e.g., 1 teaspoon of salt)",
                        },
                    new Unit 
                    { 
                        Name = "szklanka", 
                        Description = "Glass/cup - approximately 250ml",
                        PromptDescription = "Use for larger amounts of dry or liquid ingredients (e.g., 1 cup of rice, 2 cups of water)",
                        },
                    new Unit 
                    { 
                        Name = "opakowanie", 
                        Description = "Package or container unit",
                        PromptDescription = "Use for pre-packaged ingredients (e.g., 1 package of cream, 1 pack of yeast)",
                        },
                    new Unit 
                    { 
                        Name = "garść", 
                        Description = "Handful - approximate amount",
                        PromptDescription = "Use for approximate amounts of small items (e.g., a handful of nuts, herbs, or berries)",
                    },
                    new Unit 
                    { 
                        Name = "plasterek", 
                        Description = "Slice - thin piece cut from something",
                        PromptDescription = "Use for sliced ingredients (e.g., 2 slices of bread, 4 slices of cheese, 3 slices of lemon)",
                    },
                    new Unit 
                    { 
                        Name = "kostka", 
                        Description = "Cube or small block",
                        PromptDescription = "Use for cube-shaped items or bouillon cubes (e.g., 1 cube of butter, 2 bouillon cubes)",
                    }
                    };
                
                await dbContext.Units.AddRangeAsync(units);
                await dbContext.SaveChangesAsync();
                logger.LogInformation("Units seeded successfully");
            }

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