using inzynierka.Products.Services;
using inzynierka.ShoppingList.Extensions;
using inzynierka.ShoppingList.Model;
using inzynierka.ShoppingList.Repositories;
using inzynierka.ShoppingList.Requests;
using inzynierka.ShoppingList.Responses;
using inzynierka.Units.Services;

namespace inzynierka.ShoppingList.Services;

public class ShoppingListService(
    IShoppingListRepository repository, 
    IProductService productService,
    IUnitService unitService,
    ILogger<ShoppingListService> logger)
    : IShoppingListService {
    public async Task<ShoppingListResponse?> GetShoppingListAsync(string userId)
    {
        try
        {
            var shoppingList = await repository.GetByUserIdAsync(userId);
            
            if (shoppingList == null)
            {
                shoppingList = new Model.ShoppingList
                {
                    UserId = userId
                };
                
                shoppingList = await repository.CreateAsync(shoppingList);
            }

            return shoppingList.ToResponse();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting shopping list for user {UserId}", userId);
            throw;
        }
    }

    public async Task<AddProductResult> AddProductToShoppingListAsync(string userId, AddProductToShoppingListRequest request)
    {
        try
        {
            // Check if product exists only if ProductId is provided
            if (request.ProductId.HasValue)
            {
                var productResult = await productService.GetProductAsync(request.ProductId.Value.ToString());
                if (!productResult.Success)
                {
                    return new AddProductResult
                    {
                        Success = false,
                        Message = "Product not found"
                    };
                }
            }

            // Check if unit exists
            var unit = await unitService.GetUnitAsync(request.UnitId);
            if (unit == null)
            {
                return new AddProductResult
                {
                    Success = false,
                    Message = "Unit not found"
                };
            }

            // Get or create shopping list
            var shoppingList = await repository.GetByUserIdAsync(userId);
            
            if (shoppingList == null)
            {
                shoppingList = new Model.ShoppingList
                {
                    UserId = userId
                };
                
                shoppingList = await repository.CreateAsync(shoppingList);
            }

            ShoppingListItem? existingItem = null;
            if (request.ProductId.HasValue)
            {
                existingItem = shoppingList.Items.FirstOrDefault(i => 
                    i.ProductId == request.ProductId && i.UnitId == request.UnitId);
            }
            else
            {
                existingItem = shoppingList.Items.FirstOrDefault(i => 
                    i.ProductId == null && 
                    i.ProductName.Equals(request.ProductName, StringComparison.OrdinalIgnoreCase) && 
                    i.UnitId == request.UnitId);
            }
            
            if (existingItem != null)
            {
                existingItem.Quantity += request.Quantity;
                var updatedItem = await repository.UpdateItemAsync(existingItem);
                
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
                ProductName = request.ProductName,
                Quantity = request.Quantity,
                UnitId = request.UnitId
            };

            shoppingList.Items.Add(newItem);
            var addedItem = await repository.AddItemAsync(newItem);

            return new AddProductResult
            {
                Success = true,
                Message = "Product added to shopping list",
                Item = addedItem.ToResponse()
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding product to shopping list for user {UserId}", userId);
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
            var item = await repository.GetItemAsync(itemId);
            
            if (item == null)
            {
                return new AddProductResult
                {
                    Success = false,
                    Message = "Shopping list item not found"
                };
            }

            var unit = await unitService.GetUnitAsync(request.UnitId);
            if (unit == null)
            {
                return new AddProductResult
                {
                    Success = false,
                    Message = "Unit not found"
                };
            }

            var shoppingList = await repository.GetByUserIdAsync(userId);
            if (shoppingList == null || !shoppingList.Items.Any(i => i.Id == itemId))
            {
                return new AddProductResult
                {
                    Success = false,
                    Message = "Unauthorized or item not found in your shopping list"
                };
            }

            item.Quantity = request.Quantity;
            item.UnitId = request.UnitId;
            var updatedItem = await repository.UpdateItemAsync(item);

            return new AddProductResult
            {
                Success = true,
                Message = "Shopping list item updated",
                Item = updatedItem.ToResponse()
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating shopping list item {ItemId} for user {UserId}", itemId, userId);
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
            var shoppingList = await repository.GetByUserIdAsync(userId);
            
            if (shoppingList == null || !shoppingList.Items.Any(i => i.Id == itemId))
            {
                return false;
            }

            await repository.DeleteItemAsync(itemId);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing product from shopping list for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> ClearShoppingListAsync(string userId)
    {
        try
        {
            var shoppingList = await repository.GetByUserIdAsync(userId);
            
            if (shoppingList == null)
            {
                return false;
            }

            foreach (var item in shoppingList.Items.ToList())
            {
                await repository.DeleteItemAsync(item.Id);
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error clearing shopping list for user {UserId}", userId);
            throw;
        }
    }

}

