using inzynierka.MealPlans.Services;
using inzynierka.MealPlans.Repositories;
using inzynierka.MealPlans.Model;
using inzynierka.MealPlans.Requests;
using inzynierka.MealPlans.Responses;
using inzynierka.Recipes.Services;
using inzynierka.Recipes.Model;
using Microsoft.Extensions.Logging;
using Moq;

namespace inzynierka.Tests.MealPlanTests
{
    public class MealPlanServiceTests
    {
        private readonly Mock<IMealPlanRepository> _mockRepository;
        private readonly Mock<IRecipeService> _mockRecipeService;
        private readonly Mock<ILogger<MealPlanService>> _mockLogger;
        private readonly MealPlanService _service;

        public MealPlanServiceTests()
        {
            _mockRepository = new Mock<IMealPlanRepository>();
            _mockRecipeService = new Mock<IRecipeService>();
            _mockLogger = new Mock<ILogger<MealPlanService>>();
            
            _service = new MealPlanService(
                _mockRepository.Object,
                _mockRecipeService.Object);
        }

        [Fact]
        public async Task AddMealPlanAsync_InvalidMealName_ReturnsFailure()
        {
            // Arrange
            var userId = "user123";
            var request = new CreateMealPlanRequest
            {
                MealName = "InvalidMeal",
                Date = DateTime.Now,
                RecipeId = 1
            };

            // Act
            var result = await _service.AddMealPlanAsync(userId, request);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Invalid meal name", result.Message);
            _mockRepository.Verify(r => r.AddMealPlanAsync(It.IsAny<MealPlan>()), Times.Never);
        }
        
        [Fact]
        public async Task GetMealPlansForDateAsync_NoMealPlans_ReturnsEmptyList()
        {
            // Arrange
            var userId = "user123";
            var date = new DateTime(2024, 1, 15);

            _mockRepository.Setup(r => r.GetMealPlansForUserAsync(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                          .ReturnsAsync(new List<MealPlan>());

            // Act
            var result = await _service.GetMealPlansForDateAsync(userId, date);

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.MealPlans);
        }

        [Fact]
        public async Task DeleteMealPlanAsync_ValidMealPlan_DeletesSuccessfully()
        {
            // Arrange
            var userId = "user123";
            var mealPlanId = 1;
            
            var mealPlan = new MealPlan
            {
                Id = mealPlanId,
                Name = "Breakfast",
                UserId = userId
            };

            _mockRepository.Setup(r => r.GetMealPlanAsync(mealPlanId))
                          .ReturnsAsync(mealPlan);

            _mockRepository.Setup(r => r.DeleteMealPlanAsync(mealPlanId))
                          .Returns(Task.CompletedTask);

            // Act
            var result = await _service.DeleteMealPlanAsync(userId, mealPlanId);

            // Assert
            Assert.True(result);
            _mockRepository.Verify(r => r.DeleteMealPlanAsync(mealPlanId), Times.Once);
        }

        [Fact]
        public async Task DeleteMealPlanAsync_NonExistentMealPlan_ReturnsFalse()
        {
            // Arrange
            var userId = "user123";
            var mealPlanId = 999;

            _mockRepository.Setup(r => r.GetMealPlanAsync(mealPlanId))
                          .ReturnsAsync((MealPlan?)null);

            // Act
            var result = await _service.DeleteMealPlanAsync(userId, mealPlanId);

            // Assert
            Assert.False(result);
            _mockRepository.Verify(r => r.DeleteMealPlanAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task DeleteMealPlanAsync_UnauthorizedUser_ReturnsFalse()
        {
            // Arrange
            var userId = "user123";
            var otherUserId = "user456";
            var mealPlanId = 1;
            
            var mealPlan = new MealPlan
            {
                Id = mealPlanId,
                Name = "Breakfast",
                UserId = otherUserId // Different user
            };

            _mockRepository.Setup(r => r.GetMealPlanAsync(mealPlanId))
                          .ReturnsAsync(mealPlan);

            // Act
            var result = await _service.DeleteMealPlanAsync(userId, mealPlanId);

            // Assert
            Assert.False(result);
            _mockRepository.Verify(r => r.DeleteMealPlanAsync(It.IsAny<int>()), Times.Never);
        }

        [Theory]
        [InlineData("Breakfast")]
        [InlineData("Lunch")]
        [InlineData("Dinner")]
        [InlineData("Snack")]
        public async Task AddMealPlanAsync_ValidMealNames_AcceptsAllValidNames(string mealName)
        {
            // Arrange
            var userId = "user123";
            var request = new CreateMealPlanRequest
            {
                MealName = mealName,
                Date = DateTime.Now,
                RecipeId = null // Testing without recipe
            };

            _mockRepository.Setup(r => r.GetMealPlansForUserAsync(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                          .ReturnsAsync(new List<MealPlan>());

            _mockRepository.Setup(r => r.AddMealPlanAsync(It.IsAny<MealPlan>()))
                          .Returns(Task.CompletedTask);

            // Act
            var result = await _service.AddMealPlanAsync(userId, request);

            // Assert
            Assert.True(result.Success);
            _mockRepository.Verify(r => r.AddMealPlanAsync(It.Is<MealPlan>(mp => mp.Name == mealName)), Times.Once);
        }

        [Fact]
        public async Task AddMealPlanAsync_WithoutRecipe_AddsSuccessfully()
        {
            // Arrange
            var userId = "user123";
            var request = new CreateMealPlanRequest
            {
                MealName = "Breakfast",
                Date = DateTime.Now,
                RecipeId = null
            };

            _mockRepository.Setup(r => r.GetMealPlansForUserAsync(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                          .ReturnsAsync(new List<MealPlan>());

            _mockRepository.Setup(r => r.AddMealPlanAsync(It.IsAny<MealPlan>()))
                          .Returns(Task.CompletedTask);

            // Act
            var result = await _service.AddMealPlanAsync(userId, request);

            // Assert
            Assert.True(result.Success);
            _mockRepository.Verify(r => r.AddMealPlanAsync(It.Is<MealPlan>(mp => mp.RecipeId == null)), Times.Once);
        }
    }
}