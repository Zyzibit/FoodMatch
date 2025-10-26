namespace inzynierka.Products.Services.Models;

public class ProductResult
{
    public bool Success { get; set; }
    public ProductInfo? Product { get; set; }
    public string? ErrorMessage { get; set; }
}