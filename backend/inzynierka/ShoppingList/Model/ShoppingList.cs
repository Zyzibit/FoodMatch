using inzynierka.Users.Model;

namespace inzynierka.ShoppingList.Model;

public class ShoppingList
{
    public int Id { get; set; }
    public required string UserId { get; set; } = string.Empty;
    public User? User { get; set; }
    public List<ShoppingListItem> Items { get; set; } = new();
}