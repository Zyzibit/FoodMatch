using inzynierka.Recipes.Services;
using inzynierka.Recipes.Repositories;
using inzynierka.Recipes.Model;
using inzynierka.Recipes.Requests;
using inzynierka.Recipes.Responses;
using inzynierka.Products.Services;
using inzynierka.Units.Services;
using inzynierka.Users.Services;
using inzynierka.UserPreferences.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace inzynierka.Tests.RecipesTests
{
    public class RecipeServiceTests
    {
        private readonly Mock<IRecipeRepository> _mockRepository;
        private readonly Mock<IRecipeGeneratorService> _mockRecipeGeneratorService;
        private readonly Mock<IProductService> _mockProductService;
        private readonly Mock<IUnitService> _mockUnitService;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IUserPreferencesService> _mockUserPreferencesService;
        private readonly Mock<ILogger<RecipeService>> _mockLogger;
        private readonly RecipeService _service;

        public RecipeServiceTests()
        {
            _mockRepository = new Mock<IRecipeRepository>();
            _mockRecipeGeneratorService = new Mock<IRecipeGeneratorService>();
            _mockProductService = new Mock<IProductService>();
            _mockUnitService = new Mock<IUnitService>();
            _mockUserService = new Mock<IUserService>();
            _mockUserPreferencesService = new Mock<IUserPreferencesService>();
            _mockLogger = new Mock<ILogger<RecipeService>>();
            
            _service = new RecipeService(
                _mockRepository.Object,
                _mockLogger.Object,
                _mockUserService.Object,
                _mockUserPreferencesService.Object,
                _mockRecipeGeneratorService.Object,
                _mockProductService.Object,
                _mockUnitService.Object);
        }

        [Fact]
        public async Task GetRecipeAsync_WithValidId_ShouldReturnRecipeDto()
        {
            // Arrange
            var recipeId = 1;
            var recipe = new Recipe
            {
                Id = recipeId,
                UserId = "user-123",
                Title = "Pasta Carbonara",
                Instructions = "Cook pasta, make sauce, combine",
                Source = RecipeSource.User,
                TotalWeightGrams = 350,
                Calories = 650,
                Protein = 30,
                Carbohydrates = 60,
                Fats = 35,
                Ingredients = new List<RecipeIngredient>(),
                CreatedAt = DateTime.UtcNow
            };
            _mockRepository.Setup(r => r.GetRecipeByIdAsync(recipeId))
                          .ReturnsAsync(recipe);

            // Act
            var result = await _service.GetRecipeAsync(recipeId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Pasta Carbonara", result.Title);
            Assert.Equal(30, result.Proteins);
            _mockRepository.Verify(r => r.GetRecipeByIdAsync(recipeId), Times.Once);
        }

        [Fact]
        public async Task GetUserRecipesAsync_WithValidUserId_ShouldReturnRecipeList()
        {
            // Arrange
            var userId = "user-123";
            var recipes = new List<Recipe>
            {
                new Recipe
                {
                    Id = 1,
                    UserId = userId,
                    Title = "Recipe 1",
                    Instructions = "Instructions 1",
                    Source = RecipeSource.User,
                    TotalWeightGrams = 300,
                    Calories = 400,
                    Protein = 20,
                    Carbohydrates = 50,
                    Fats = 10,
                    Ingredients = new List<RecipeIngredient>(),
                    CreatedAt = DateTime.UtcNow
                },
                new Recipe
                {
                    Id = 2,
                    UserId = userId,
                    Title = "Recipe 2",
                    Instructions = "Instructions 2",
                    Source = RecipeSource.User,
                    TotalWeightGrams = 400,
                    Calories = 500,
                    Protein = 25,
                    Carbohydrates = 55,
                    Fats = 15,
                    Ingredients = new List<RecipeIngredient>(),
                    CreatedAt = DateTime.UtcNow
                }
            };

            _mockRepository.Setup(r => r.GetUserRecipesAsync(userId, It.IsAny<int>(), It.IsAny<int>()))
                          .ReturnsAsync((recipes, recipes.Count));

            // Act
            var result = await _service.GetUserRecipesAsync(userId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Recipes.Count);
            Assert.Equal(2, result.TotalCount);
        }

        [Fact]
        public async Task GetPublicRecipesAsync_ShouldReturnOnlyPublicRecipes()
        {
            // Arrange
            var publicRecipes = new List<Recipe>
            {
                new Recipe
                {
                    Id = 1,
                    UserId = "user-456",
                    Title = "Public Recipe",
                    Instructions = "Public instructions",
                    Source = RecipeSource.User,
                    TotalWeightGrams = 300,
                    Calories = 450,
                    Protein = 25,
                    Carbohydrates = 50,
                    Fats = 12,
                    IsPublic = true,
                    Ingredients = new List<RecipeIngredient>(),
                    CreatedAt = DateTime.UtcNow
                }
            };

            _mockRepository.Setup(r => r.GetPublicRecipesAsync(It.IsAny<int>(), It.IsAny<int>()))
                          .ReturnsAsync((publicRecipes, publicRecipes.Count));

            // Act
            var result = await _service.GetPublicRecipesAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Recipes);
            Assert.Equal("Public Recipe", result.Recipes[0].Title);
        }

        [Fact]
        public async Task DeleteRecipeAsync_WithValidUserAndRecipe_ShouldDeleteSuccessfully()
        {
            // Arrange
            var userId = "user-123";
            var recipeId = 1;
            var recipe = new Recipe
            {
                Id = recipeId,
                UserId = userId,
                Title = "Recipe to Delete",
                Instructions = "Instructions",
                Source = RecipeSource.User,
                TotalWeightGrams = 300,
                Calories = 400,
                Protein = 20,
                Carbohydrates = 50,
                Fats = 10,
                Ingredients = new List<RecipeIngredient>(),
                CreatedAt = DateTime.UtcNow
            };

            _mockRepository.Setup(r => r.GetRecipeByIdAsync(recipeId))
                          .ReturnsAsync(recipe);

            _mockRepository.Setup(r => r.DeleteRecipeAsync(recipeId))
                          .Returns(Task.CompletedTask);

            // Act
            var result = await _service.DeleteRecipeAsync(userId, recipeId);

            // Assert
            Assert.True(result);
            _mockRepository.Verify(r => r.DeleteRecipeAsync(recipeId), Times.Once);
        }
        
    }
}

