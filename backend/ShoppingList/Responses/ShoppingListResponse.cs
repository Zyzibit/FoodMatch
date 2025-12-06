namespace inzynierka.ShoppingList.Responses;

public class ShoppingListResponse
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public List<ShoppingListItemResponse> Items { get; set; } = new();
}

public class ShoppingListItemResponse
{
    public int Id { get; set; }
    public decimal Quantity { get; set; }
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductCode { get; set; }
    public string? ImageUrl { get; set; }
    public string? Source { get; set; } = string.Empty;
    public string? Brands { get; set; }
}

