namespace inzynierka.Products.Services.Models;

public class ProductSearchRequest
{
    public string? Query { get; set; }
    public string? Categories { get; set; }
    public string? Allergens { get; set; }
    public string? Ingredients { get; set; }
    public string? Brand { get; set; }
    public int Limit { get; set; } = 10;
    public int Offset { get; set; } = 0;
}