using inzynierka.Data;
using Microsoft.EntityFrameworkCore;

namespace inzynierka.ShoppingList.Repositories;

public class ShoppingListRepository : IShoppingListRepository
{
    private readonly AppDbContext _context;

    public ShoppingListRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Model.ShoppingList?> GetByUserIdAsync(string userId)
    {
        return await _context.ShoppingLists
            .Include(sl => sl.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(sl => sl.UserId == userId);
    }

    public async Task<Model.ShoppingList> CreateAsync(Model.ShoppingList shoppingList)
    {
        _context.ShoppingLists.Add(shoppingList);
        await _context.SaveChangesAsync();
        return shoppingList;
    }

    public async Task<ShoppingListItem?> GetItemAsync(int itemId)
    {
        return await _context.ShoppingListItems
            .Include(i => i.Product)
            .FirstOrDefaultAsync(i => i.Id == itemId);
    }

    public async Task<ShoppingListItem> AddItemAsync(ShoppingListItem item)
    {
        _context.ShoppingListItems.Add(item);
        await _context.SaveChangesAsync();
        
        // Load the product details
        await _context.Entry(item)
            .Reference(i => i.Product)
            .LoadAsync();
            
        return item;
    }

    public async Task<ShoppingListItem> UpdateItemAsync(ShoppingListItem item)
    {
        _context.ShoppingListItems.Update(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task DeleteItemAsync(int itemId)
    {
        var item = await _context.ShoppingListItems.FindAsync(itemId);
        if (item != null)
        {
            _context.ShoppingListItems.Remove(item);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ProductExistsAsync(int productId)
    {
        return await _context.Products.AnyAsync(p => p.Id == productId);
    }
}

