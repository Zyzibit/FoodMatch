using inzynierka.ShoppingList.Extensions;
using inzynierka.ShoppingList.Repositories;
using inzynierka.ShoppingList.Requests;
using inzynierka.ShoppingList.Responses;

namespace inzynierka.ShoppingList.Services;

public class ShoppingListService : IShoppingListService
{
    private readonly IShoppingListRepository _repository;
    private readonly ILogger<ShoppingListService> _logger;

    public ShoppingListService(IShoppingListRepository repository, ILogger<ShoppingListService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ShoppingListResponse?> GetShoppingListAsync(string userId)
    {
        try
        {
            var shoppingList = await _repository.GetByUserIdAsync(userId);
            
            if (shoppingList == null)
            {
                shoppingList = new Model.ShoppingList
                {
                    UserId = userId
                };
                
                shoppingList = await _repository.CreateAsync(shoppingList);
            }

            return shoppingList.ToResponse();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting shopping list for user {UserId}", userId);
            throw;
        }
    }

    public async Task<AddProductResult> AddProductToShoppingListAsync(string userId, AddProductToShoppingListRequest request)
    {
        try
        {
            // Check if product exists
            var productExists = await _repository.ProductExistsAsync(request.ProductId);
            if (!productExists)
            {
                return new AddProductResult
                {
                    Success = false,
                    Message = "Product not found"
                };
            }

            // Get or create shopping list
            var shoppingList = await _repository.GetByUserIdAsync(userId);
            
            if (shoppingList == null)
            {
                shoppingList = new Model.ShoppingList
                {
                    UserId = userId
                };
                
                shoppingList = await _repository.CreateAsync(shoppingList);
            }

            var existingItem = shoppingList.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
            
            if (existingItem != null)
            {
                existingItem.Quantity += request.Quantity;
                var updatedItem = await _repository.UpdateItemAsync(existingItem);
                
                return new AddProductResult
                {
                    Success = true,
                    Message = "Product quantity updated",
                    Item = updatedItem.ToResponse()
                };
            }

            var newItem = new ShoppingListItem
            {
                ProductId = request.ProductId,
                Quantity = request.Quantity
                
            };

            shoppingList.Items.Add(newItem);
            var addedItem = await _repository.AddItemAsync(newItem);

            return new AddProductResult
            {
                Success = true,
                Message = "Product added to shopping list",
                Item = addedItem.ToResponse()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding product to shopping list for user {UserId}", userId);
            return new AddProductResult
            {
                Success = false,
                Message = "An error occurred while adding the product"
            };
        }
    }

    public async Task<AddProductResult> UpdateShoppingListItemAsync(string userId, int itemId, UpdateShoppingListItemRequest request)
    {
        try
        {
            var item = await _repository.GetItemAsync(itemId);
            
            if (item == null)
            {
                return new AddProductResult
                {
                    Success = false,
                    Message = "Shopping list item not found"
                };
            }

            // Verify the item belongs to the user's shopping list
            var shoppingList = await _repository.GetByUserIdAsync(userId);
            if (shoppingList == null || !shoppingList.Items.Any(i => i.Id == itemId))
            {
                return new AddProductResult
                {
                    Success = false,
                    Message = "Unauthorized or item not found in your shopping list"
                };
            }

            item.Quantity = request.Quantity;
            var updatedItem = await _repository.UpdateItemAsync(item);

            return new AddProductResult
            {
                Success = true,
                Message = "Shopping list item updated",
                Item = updatedItem.ToResponse()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating shopping list item {ItemId} for user {UserId}", itemId, userId);
            return new AddProductResult
            {
                Success = false,
                Message = "An error occurred while updating the item"
            };
        }
    }

    public async Task<bool> RemoveProductFromShoppingListAsync(string userId, int itemId)
    {
        try
        {
            var shoppingList = await _repository.GetByUserIdAsync(userId);
            
            if (shoppingList == null || !shoppingList.Items.Any(i => i.Id == itemId))
            {
                return false;
            }

            await _repository.DeleteItemAsync(itemId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing product from shopping list for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> ClearShoppingListAsync(string userId)
    {
        try
        {
            var shoppingList = await _repository.GetByUserIdAsync(userId);
            
            if (shoppingList == null)
            {
                return false;
            }

            foreach (var item in shoppingList.Items.ToList())
            {
                await _repository.DeleteItemAsync(item.Id);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing shopping list for user {UserId}", userId);
            throw;
        }
    }

}

