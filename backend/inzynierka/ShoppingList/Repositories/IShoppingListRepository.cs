using inzynierka.ShoppingList.Model;

namespace inzynierka.ShoppingList.Repositories;

public interface IShoppingListRepository
{
    Task<Model.ShoppingList?> GetByUserIdAsync(string userId);
    Task<Model.ShoppingList> CreateAsync(Model.ShoppingList shoppingList);
    Task<ShoppingListItem?> GetItemAsync(int itemId);
    Task<ShoppingListItem> AddItemAsync(ShoppingListItem item);
    Task<ShoppingListItem> UpdateItemAsync(ShoppingListItem item);
    Task DeleteItemAsync(int itemId);
}


