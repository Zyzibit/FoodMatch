using inzynierka.Users.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using inzynierka.Recipes.Model;
using inzynierka.Units.Models;
using inzynierka.Products.Model;

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
                    new Unit
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

            if (!await dbContext.Products.AnyAsync())
            {
                logger.LogInformation("Seeding products from Open Food Facts...");
                var products = new List<Product>
                {
                    new Product
                    {
                        Code = "5900512400822",
                        ProductName = "Mleko UHT 2% tłuszczu",
                        Brands = "Łaciate",
                        Language = "pl",
                        LanguageCode = "pl",
                        Source = ProductSource.OpenFoodFacts,
                        EnergyKcal100g = 49,
                        Fat100g = 2.0,
                        SaturatedFat100g = 1.3,
                        Carbohydrates100g = 4.8,
                        Sugars100g = 4.8,
                        Proteins100g = 3.2,
                        Salt100g = 0.1,
                        IsVegetarian = "yes",
                        IsVegan = "no",
                        NutritionGrade = "b",
                        NovaGroup = 1,
                        ServingSize = "250ml",
                        ImageUrl = "https://images.openfoodfacts.org/images/products/590/051/240/0822/front_pl.jpg",
                        LastUpdated = DateTime.UtcNow
                    },
                    new Product
                    {
                        Code = "5900512000107",
                        ProductName = "Ser żółty Gouda",
                        Brands = "Mlekovita",
                        Language = "pl",
                        LanguageCode = "pl",
                        Source = ProductSource.OpenFoodFacts,
                        EnergyKcal100g = 364,
                        Fat100g = 28.0,
                        SaturatedFat100g = 18.0,
                        Carbohydrates100g = 0.0,
                        Sugars100g = 0.0,
                        Proteins100g = 29.0,
                        Salt100g = 1.8,
                        IsVegetarian = "yes",
                        IsVegan = "no",
                        NutritionGrade = "d",
                        NovaGroup = 3,
                        ServingSize = "30g",
                        ImageUrl = "https://images.openfoodfacts.org/images/products/590/051/200/0107/front_pl.jpg",
                        LastUpdated = DateTime.UtcNow
                    },
                    new Product
                    {
                        Code = "5900783006297",
                        ProductName = "Chleb pszenny",
                        Brands = "Putka",
                        Language = "pl",
                        LanguageCode = "pl",
                        Source = ProductSource.OpenFoodFacts,
                        EnergyKcal100g = 265,
                        Fat100g = 1.2,
                        SaturatedFat100g = 0.3,
                        Carbohydrates100g = 52.0,
                        Sugars100g = 3.5,
                        Fiber100g = 3.2,
                        Proteins100g = 8.5,
                        Salt100g = 1.2,
                        IsVegetarian = "yes",
                        IsVegan = "yes",
                        NutritionGrade = "c",
                        NovaGroup = 4,
                        ServingSize = "50g",
                        ImageUrl = "https://images.openfoodfacts.org/images/products/590/078/300/6297/front_pl.jpg",
                        LastUpdated = DateTime.UtcNow
                    },
                    new Product
                    {
                        Code = "5900396011619",
                        ProductName = "Jogurt naturalny",
                        Brands = "Danone",
                        Language = "pl",
                        LanguageCode = "pl",
                        Source = ProductSource.OpenFoodFacts,
                        EnergyKcal100g = 61,
                        Fat100g = 3.0,
                        SaturatedFat100g = 2.0,
                        Carbohydrates100g = 4.7,
                        Sugars100g = 4.7,
                        Proteins100g = 4.5,
                        Salt100g = 0.13,
                        IsVegetarian = "yes",
                        IsVegan = "no",
                        NutritionGrade = "a",
                        NovaGroup = 1,
                        ServingSize = "150g",
                        ImageUrl = "https://images.openfoodfacts.org/images/products/590/039/601/1619/front_pl.jpg",
                        LastUpdated = DateTime.UtcNow
                    },
                    new Product
                    {
                        Code = "5900783000622",
                        ProductName = "Masło extra",
                        Brands = "Mlekovita",
                        Language = "pl",
                        LanguageCode = "pl",
                        Source = ProductSource.OpenFoodFacts,
                        EnergyKcal100g = 748,
                        Fat100g = 82.5,
                        SaturatedFat100g = 54.0,
                        Carbohydrates100g = 0.6,
                        Sugars100g = 0.6,
                        Proteins100g = 0.7,
                        Salt100g = 0.01,
                        IsVegetarian = "yes",
                        IsVegan = "no",
                        NutritionGrade = "e",
                        NovaGroup = 3,
                        ServingSize = "10g",
                        ImageUrl = "https://images.openfoodfacts.org/images/products/590/078/300/0622/front_pl.jpg",
                        LastUpdated = DateTime.UtcNow
                    },
                    new Product
                    {
                        Code = "5900334003058",
                        ProductName = "Makaron penne",
                        Brands = "Lubella",
                        Language = "pl",
                        LanguageCode = "pl",
                        Source = ProductSource.OpenFoodFacts,
                        EnergyKcal100g = 350,
                        Fat100g = 1.5,
                        SaturatedFat100g = 0.3,
                        Carbohydrates100g = 70.0,
                        Sugars100g = 3.5,
                        Fiber100g = 3.0,
                        Proteins100g = 12.0,
                        Salt100g = 0.01,
                        IsVegetarian = "yes",
                        IsVegan = "yes",
                        NutritionGrade = "a",
                        NovaGroup = 1,
                        ServingSize = "100g",
                        ImageUrl = "https://images.openfoodfacts.org/images/products/590/033/400/3058/front_pl.jpg",
                        LastUpdated = DateTime.UtcNow
                    },
                    new Product
                    {
                        Code = "5906747071097",
                        ProductName = "Olej rzepakowy",
                        Brands = "Kujawski",
                        Language = "pl",
                        LanguageCode = "pl",
                        Source = ProductSource.OpenFoodFacts,
                        EnergyKcal100g = 900,
                        Fat100g = 100.0,
                        SaturatedFat100g = 7.0,
                        Carbohydrates100g = 0.0,
                        Sugars100g = 0.0,
                        Proteins100g = 0.0,
                        Salt100g = 0.0,
                        IsVegetarian = "yes",
                        IsVegan = "yes",
                        NutritionGrade = "d",
                        NovaGroup = 2,
                        ServingSize = "15ml",
                        ImageUrl = "https://images.openfoodfacts.org/images/products/590/674/707/1097/front_pl.jpg",
                        LastUpdated = DateTime.UtcNow
                    },
                    new Product
                    {
                        Code = "5900783002916",
                        ProductName = "Jaja świeże M",
                        Brands = "Fermy Drobiu Woźniak",
                        Language = "pl",
                        LanguageCode = "pl",
                        Source = ProductSource.OpenFoodFacts,
                        EnergyKcal100g = 143,
                        Fat100g = 9.7,
                        SaturatedFat100g = 3.1,
                        Carbohydrates100g = 0.7,
                        Sugars100g = 0.7,
                        Proteins100g = 12.5,
                        Salt100g = 0.35,
                        IsVegetarian = "yes",
                        IsVegan = "no",
                        NutritionGrade = "b",
                        NovaGroup = 1,
                        ServingSize = "1 sztuka (60g)",
                        ImageUrl = "https://images.openfoodfacts.org/images/products/590/078/300/2916/front_pl.jpg",
                        LastUpdated = DateTime.UtcNow
                    }
                };

                await dbContext.Products.AddRangeAsync(products);
                await dbContext.SaveChangesAsync();
                logger.LogInformation("Products seeded successfully");
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
                    UserName = "admin111222",
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