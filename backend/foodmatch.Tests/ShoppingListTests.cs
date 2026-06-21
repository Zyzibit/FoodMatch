using inzynierka.ShoppingList.Services;
using inzynierka.ShoppingList.Repositories;
using inzynierka.ShoppingList.Model;
using inzynierka.ShoppingList.Requests;
using inzynierka.ShoppingList.Responses;
using inzynierka.Products.Services;
using inzynierka.Units.Services;
using inzynierka.Units.Models;
using inzynierka.Products.Responses;
using Microsoft.Extensions.Logging;
using Moq;

namespace inzynierka.Tests.ShoppingListTests
{
    public class ShoppingListServiceTests
    {
        private readonly Mock<IShoppingListRepository> _mockRepository;
        private readonly Mock<IProductService> _mockProductService;
        private readonly Mock<IUnitService> _mockUnitService;
        private readonly Mock<ILogger<ShoppingListService>> _mockLogger;
        private readonly ShoppingListService _service;

        public ShoppingListServiceTests()
        {
            _mockRepository = new Mock<IShoppingListRepository>();
            _mockProductService = new Mock<IProductService>();
            _mockUnitService = new Mock<IUnitService>();
            _mockLogger = new Mock<ILogger<ShoppingListService>>();
            
            _service = new ShoppingListService(
                _mockRepository.Object,
                _mockProductService.Object,
                _mockUnitService.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task GetShoppingListAsync_ExistingList_ReturnsShoppingListResponse()
        {
            // Arrange
            var userId = "user123";
            var shoppingList = new ShoppingList.Model.ShoppingList
            {
                Id = 1,
                UserId = userId,
                Items = new List<ShoppingListItem>()
            };

            _mockRepository.Setup(r => r.GetByUserIdAsync(userId))
                          .ReturnsAsync(shoppingList);

            // Act
            var result = await _service.GetShoppingListAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(userId, result.UserId);
            Assert.Empty(result.Items);
        }

        [Fact]
        public async Task GetShoppingListAsync_NonExistingList_CreatesAndReturnsNewList()
        {
            // Arrange
            var userId = "user123";
            var newShoppingList = new ShoppingList.Model.ShoppingList
            {
                Id = 1,
                UserId = userId,
                Items = new List<ShoppingListItem>()
            };

            _mockRepository.Setup(r => r.GetByUserIdAsync(userId))
                          .ReturnsAsync((ShoppingList.Model.ShoppingList?)null);
            
            _mockRepository.Setup(r => r.CreateAsync(It.IsAny<ShoppingList.Model.ShoppingList>()))
                          .ReturnsAsync(newShoppingList);

            // Act
            var result = await _service.GetShoppingListAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(userId, result.UserId);
            _mockRepository.Verify(r => r.CreateAsync(It.IsAny<ShoppingList.Model.ShoppingList>()), Times.Once);
        }
        

        [Fact]
        public async Task AddProductToShoppingListAsync_InvalidProduct_ReturnsFailure()
        {
            // Arrange
            var userId = "user123";
            var productId = 1;
            var unitId = 1;

            var request = new AddProductToShoppingListRequest
            {
                ProductId = productId,
                Quantity = 2.5m,
                UnitId = unitId
            };

            var productResult = new ProductResult { Success = false };

            _mockProductService.Setup(p => p.GetProductAsync(productId.ToString()))
                              .ReturnsAsync(productResult);

            // Act
            var result = await _service.AddProductToShoppingListAsync(userId, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Product not found", result.Message);
        }

        [Fact]
        public async Task UpdateShoppingListItemAsync_NonExistentItem_ReturnsFailure()
        {
            // Arrange
            var userId = "user123";
            var itemId = 999;
            var request = new UpdateShoppingListItemRequest
            {
                Quantity = 3.0m,
                UnitId = 1
            };

            _mockRepository.Setup(r => r.GetItemAsync(itemId))
                          .ReturnsAsync((ShoppingListItem?)null);

            // Act
            var result = await _service.UpdateShoppingListItemAsync(userId, itemId, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Shopping list item not found", result.Message);
        }

        [Fact]
        public async Task RemoveProductFromShoppingListAsync_ValidItem_RemovesSuccessfully()
        {
            // Arrange
            var userId = "user123";
            var itemId = 1;

            var item = new ShoppingListItem
            {
                Id = itemId,
                ProductName = "Test Product",
                Quantity = 1.0m,
                UnitId = 1
            };

            var shoppingList = new ShoppingList.Model.ShoppingList
            {
                Id = 1,
                UserId = userId,
                Items = new List<ShoppingListItem> { item }
            };

            _mockRepository.Setup(r => r.GetByUserIdAsync(userId))
                          .ReturnsAsync(shoppingList);
            
            _mockRepository.Setup(r => r.DeleteItemAsync(itemId))
                          .Returns(Task.CompletedTask);

            // Act
            var result = await _service.RemoveProductFromShoppingListAsync(userId, itemId);

            // Assert
            Assert.True(result);
            _mockRepository.Verify(r => r.DeleteItemAsync(itemId), Times.Once);
        }

        [Fact]
        public async Task RemoveProductFromShoppingListAsync_ItemNotInUserList_ReturnsFalse()
        {
            // Arrange
            var userId = "user123";
            var itemId = 999;

            var shoppingList = new ShoppingList.Model.ShoppingList
            {
                Id = 1,
                UserId = userId,
                Items = new List<ShoppingListItem>()
            };

            _mockRepository.Setup(r => r.GetByUserIdAsync(userId))
                          .ReturnsAsync(shoppingList);

            // Act
            var result = await _service.RemoveProductFromShoppingListAsync(userId, itemId);

            // Assert
            Assert.False(result);
            _mockRepository.Verify(r => r.DeleteItemAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task ClearShoppingListAsync_ExistingList_ClearsAllItems()
        {
            // Arrange
            var userId = "user123";
            
            var items = new List<ShoppingListItem>
            {
                new() { Id = 1, ProductName = "Product 1", Quantity = 1, UnitId = 1 },
                new() { Id = 2, ProductName = "Product 2", Quantity = 2, UnitId = 1 }
            };

            var shoppingList = new ShoppingList.Model.ShoppingList
            {
                Id = 1,
                UserId = userId,
                Items = items
            };

            _mockRepository.Setup(r => r.GetByUserIdAsync(userId))
                          .ReturnsAsync(shoppingList);
            
            _mockRepository.Setup(r => r.DeleteItemAsync(It.IsAny<int>()))
                          .Returns(Task.CompletedTask);

            // Act
            var result = await _service.ClearShoppingListAsync(userId);

            // Assert
            Assert.True(result);
            _mockRepository.Verify(r => r.DeleteItemAsync(1), Times.Once);
            _mockRepository.Verify(r => r.DeleteItemAsync(2), Times.Once);
        }

        [Fact]
        public async Task ClearShoppingListAsync_NonExistingList_ReturnsFalse()
        {
            // Arrange
            var userId = "user123";

            _mockRepository.Setup(r => r.GetByUserIdAsync(userId))
                          .ReturnsAsync((ShoppingList.Model.ShoppingList?)null);

            // Act
            var result = await _service.ClearShoppingListAsync(userId);

            // Assert
            Assert.False(result);
            _mockRepository.Verify(r => r.DeleteItemAsync(It.IsAny<int>()), Times.Never);
        }
    }
}