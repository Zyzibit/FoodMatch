using inzynierka.Products.Model;
using inzynierka.Recipes.Model;
using inzynierka.Recipes.Repositories;
using inzynierka.Units.Models;
using inzynierka.Users.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace inzynierka.IntegrationTests;

public class RecipesRepositoryIntegrationTests : DatabaseIntegrationTest
{
    private RecipeRepository _repository = null!;
    private UserManager<User> _userManager = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _userManager = ServiceProvider.GetRequiredService<UserManager<User>>();
        _repository = new RecipeRepository(DbContext);
    }

    private async Task<User> CreateTestUser(string email = "test@example.com")
    {
        var user = new User
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };
        await _userManager.CreateAsync(user, "Test123!");
        return user;
    }

    private async Task<Product> CreateTestProduct(string code)
    {
        var product = new Product
        {
            Code = code,
            ProductName = $"Product {code}",
            Language = "pl"
        };
        DbContext.Products.Add(product);
        await DbContext.SaveChangesAsync();
        return product;
    }

    private async Task<Unit> CreateTestUnit(string name)
    {
        var unit = new Unit
        {
            Name = name,
            Description = $"Description for {name}",
            PromptDescription = $"Prompt for {name}"
        };
        DbContext.Units.Add(unit);
        await DbContext.SaveChangesAsync();
        return unit;
    }

    [Fact]
    public async Task AddRecipe_ShouldAddRecipeToDatabase()
    {
        // Arrange
        var recipe = new Recipe
        {
            UserId = "test-user-123",
            Title = "Spaghetti Bolognese",
            Description = "Classic Italian pasta dish",
            Instructions = "Cook pasta, prepare sauce, combine",
            Source = RecipeSource.User,
            PreparationTimeMinutes = 30,
            TotalWeightGrams = 400,
            Calories = 550,
            Protein = 25,
            Carbohydrates = 70,
            Fats = 15,
            IsPublic = false,
            Ingredients = new List<RecipeIngredient>(),
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var addedRecipe = await _repository.AddRecipeAsync(recipe);
        var result = await _repository.GetRecipeByIdAsync(addedRecipe.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Spaghetti Bolognese", result.Title);
        Assert.Equal("test-user-123", result.UserId);
        Assert.False(result.IsPublic);
    }

    [Fact]
    public async Task AddRecipeAsync_ShouldAddRecipe()
    {
        // Arrange
        var user = await CreateTestUser();
        var product = await CreateTestProduct("REC001");
        var unit = await CreateTestUnit("gram");

        var recipe = new Recipe
        {
            UserId = user.Id,
            Title = "Test Recipe",
            Instructions = "Test Instructions",
            Source = RecipeSource.User,
            TotalWeightGrams = 500,
            Calories = 250,
            Protein = 10,
            Carbohydrates = 30,
            Fats = 15,
            Ingredients = new List<RecipeIngredient>
            {
                new RecipeIngredient
                {
                    ProductId = product.Id,
                    UnitId = unit.UnitId,
                    Quantity = 100
                }
            }
        };

        // Act
        var result = await _repository.AddRecipeAsync(recipe);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("Test Recipe", result.Title);
        Assert.Equal(user.Id, result.UserId);
    }

    [Fact]
    public async Task GetRecipeByIdAsync_ShouldReturnRecipeWithIngredients()
    {
        // Arrange
        var user = await CreateTestUser("recipe@test.com");
        var product = await CreateTestProduct("REC002");
        var unit = await CreateTestUnit("kg");

        var recipe = new Recipe
        {
            UserId = user.Id,
            Title = "Recipe with Ingredients",
            Instructions = "Instructions",
            Source = RecipeSource.User,
            TotalWeightGrams = 1000,
            Calories = 500,
            Protein = 20,
            Carbohydrates = 60,
            Fats = 25,
            Ingredients = new List<RecipeIngredient>
            {
                new RecipeIngredient
                {
                    ProductId = product.Id,
                    UnitId = unit.UnitId,
                    Quantity = 200
                }
            }
        };
        await _repository.AddRecipeAsync(recipe);

        // Act
        var result = await _repository.GetRecipeByIdAsync(recipe.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(recipe.Id, result.Id);
        Assert.NotEmpty(result.Ingredients);
        Assert.Single(result.Ingredients);
        Assert.NotNull(result.Ingredients.First().Product);
        Assert.NotNull(result.Ingredients.First().Unit);
    }

    [Fact]
    public async Task GetRecipeByIdAsync_ShouldReturnNull_WhenRecipeDoesNotExist()
    {
        // Act
        var result = await _repository.GetRecipeByIdAsync(99999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateRecipeAsync_ShouldUpdateRecipe()
    {
        // Arrange
        var user = await CreateTestUser("update@test.com");
        var product = await CreateTestProduct("REC003");
        var unit = await CreateTestUnit("liter");

        var recipe = new Recipe
        {
            UserId = user.Id,
            Title = "Original Title",
            Instructions = "Original Instructions",
            Source = RecipeSource.User,
            TotalWeightGrams = 300,
            Calories = 150,
            Protein = 5,
            Carbohydrates = 20,
            Fats = 10,
            Ingredients = new List<RecipeIngredient>()
        };
        await _repository.AddRecipeAsync(recipe);

        // Act
        recipe.Title = "Updated Title";
        recipe.Instructions = "Updated Instructions";
        recipe.Calories = 200;
        var result = await _repository.UpdateRecipeAsync(recipe);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Title", result.Title);
        Assert.Equal("Updated Instructions", result.Instructions);
        Assert.Equal(200, result.Calories);
    }

    [Fact]
    public async Task DeleteRecipeAsync_ShouldDeleteRecipe()
    {
        // Arrange
        var user = await CreateTestUser("delete@test.com");
        var recipe = new Recipe
        {
            UserId = user.Id,
            Title = "Recipe to Delete",
            Instructions = "Instructions",
            Source = RecipeSource.User,
            TotalWeightGrams = 400,
            Calories = 180,
            Protein = 8,
            Carbohydrates = 25,
            Fats = 12,
            Ingredients = new List<RecipeIngredient>()
        };
        await _repository.AddRecipeAsync(recipe);
        var recipeId = recipe.Id;

        // Act
        await _repository.DeleteRecipeAsync(recipeId);
        var result = await _repository.GetRecipeByIdAsync(recipeId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllRecipesAsync_ShouldReturnPagedResults()
    {
        // Arrange
        var user = await CreateTestUser("all@test.com");
        for (int i = 0; i < 15; i++)
        {
            var recipe = new Recipe
            {
                UserId = user.Id,
                Title = $"Recipe {i}",
                Instructions = $"Instructions {i}",
                Source = RecipeSource.User,
                TotalWeightGrams = 100 * i,
                Calories = 50 * i,
                Protein = i,
                Carbohydrates = i * 2,
                Fats = i * 3,
                Ingredients = new List<RecipeIngredient>()
            };
            await _repository.AddRecipeAsync(recipe);
        }

        // Act
        var (recipes, totalCount) = await _repository.GetAllRecipesAsync(10, 0);

        // Assert
        Assert.Equal(15, totalCount);
        Assert.Equal(10, recipes.Count);
    }

    [Fact]
    public async Task GetUserRecipesAsync_ShouldReturnOnlyUserRecipes()
    {
        // Arrange
        var user1 = await CreateTestUser("user1@test.com");
        var user2 = await CreateTestUser("user2@test.com");

        for (int i = 0; i < 5; i++)
        {
            var recipe = new Recipe
            {
                UserId = user1.Id,
                Title = $"User1 Recipe {i}",
                Instructions = "Instructions",
                Source = RecipeSource.User,
                TotalWeightGrams = 100,
                Calories = 100,
                Protein = 10,
                Carbohydrates = 20,
                Fats = 5,
                Ingredients = new List<RecipeIngredient>()
            };
            await _repository.AddRecipeAsync(recipe);
        }

        for (int i = 0; i < 3; i++)
        {
            var recipe = new Recipe
            {
                UserId = user2.Id,
                Title = $"User2 Recipe {i}",
                Instructions = "Instructions",
                Source = RecipeSource.User,
                TotalWeightGrams = 100,
                Calories = 100,
                Protein = 10,
                Carbohydrates = 20,
                Fats = 5,
                Ingredients = new List<RecipeIngredient>()
            };
            await _repository.AddRecipeAsync(recipe);
        }

        // Act
        var (recipes, totalCount) = await _repository.GetUserRecipesAsync(user1.Id, 50, 0);

        // Assert
        Assert.Equal(5, totalCount);
        Assert.Equal(5, recipes.Count);
        Assert.All(recipes, r => Assert.Equal(user1.Id, r.UserId));
    }

    [Fact]
    public async Task GetPublicRecipesAsync_ShouldReturnOnlyPublicRecipes()
    {
        // Arrange
        var user = await CreateTestUser("public@test.com");

        for (int i = 0; i < 5; i++)
        {
            var recipe = new Recipe
            {
                UserId = user.Id,
                Title = $"Public Recipe {i}",
                Instructions = "Instructions",
                Source = RecipeSource.User,
                TotalWeightGrams = 100,
                Calories = 100,
                Protein = 10,
                Carbohydrates = 20,
                Fats = 5,
                IsPublic = true,
                Ingredients = new List<RecipeIngredient>()
            };
            await _repository.AddRecipeAsync(recipe);
        }

        for (int i = 0; i < 3; i++)
        {
            var recipe = new Recipe
            {
                UserId = user.Id,
                Title = $"Private Recipe {i}",
                Instructions = "Instructions",
                Source = RecipeSource.User,
                TotalWeightGrams = 100,
                Calories = 100,
                Protein = 10,
                Carbohydrates = 20,
                Fats = 5,
                IsPublic = false,
                Ingredients = new List<RecipeIngredient>()
            };
            await _repository.AddRecipeAsync(recipe);
        }

        // Act
        var (recipes, totalCount) = await _repository.GetPublicRecipesAsync(50, 0);

        // Assert
        Assert.Equal(5, totalCount);
        Assert.Equal(5, recipes.Count);
        Assert.All(recipes, r => Assert.True(r.IsPublic));
    }

    [Fact]
    public async Task GetAllRecipesAsync_ShouldRespectPagination()
    {
        // Arrange
        var user = await CreateTestUser("pagination@test.com");
        for (int i = 0; i < 25; i++)
        {
            var recipe = new Recipe
            {
                UserId = user.Id,
                Title = $"Pagination Recipe {i}",
                Instructions = "Instructions",
                Source = RecipeSource.User,
                TotalWeightGrams = 100,
                Calories = 100,
                Protein = 10,
                Carbohydrates = 20,
                Fats = 5,
                Ingredients = new List<RecipeIngredient>()
            };
            await _repository.AddRecipeAsync(recipe);
        }

        // Act
        var (firstPage, totalCount1) = await _repository.GetAllRecipesAsync(10, 0);
        var (secondPage, totalCount2) = await _repository.GetAllRecipesAsync(10, 10);

        // Assert
        Assert.Equal(25, totalCount1);
        Assert.Equal(25, totalCount2);
        Assert.Equal(10, firstPage.Count);
        Assert.Equal(10, secondPage.Count);
        Assert.NotEqual(firstPage.First().Id, secondPage.First().Id);
    }
}

