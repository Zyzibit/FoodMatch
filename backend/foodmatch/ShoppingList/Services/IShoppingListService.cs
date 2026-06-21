namespace inzynierka.ShoppingList.Services;

using Responses;
using inzynierka.ShoppingList.Requests;
public interface IShoppingListService
{
    Task<bool> ClearShoppingListAsync(string userId);
    Task<bool> RemoveProductFromShoppingListAsync(string userId, int itemId);
    Task<AddProductResult> UpdateShoppingListItemAsync(string userId, int itemId, UpdateShoppingListItemRequest request);
    Task<AddProductResult> AddProductToShoppingListAsync(string userId, AddProductToShoppingListRequest request);
    Task<ShoppingListResponse?> GetShoppingListAsync(string userId);
}



