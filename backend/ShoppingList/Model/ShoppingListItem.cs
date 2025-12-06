using inzynierka.Products.Model;

namespace inzynierka.ShoppingList;

public class ShoppingListItem
{
    public int Id { get; set; }
    public decimal Quantity { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; }
}