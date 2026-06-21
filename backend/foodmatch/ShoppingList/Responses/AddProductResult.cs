namespace inzynierka.ShoppingList.Responses;

public class AddProductResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public ShoppingListItemResponse? Item { get; set; }
}
