namespace inzynierka.Products.Contracts.Models;

public class ProductCategoryResult
{
    public bool Success { get; set; }
    public List<ProductInfo> Products { get; set; } = new();
    public int TotalCount { get; set; }
    public string? ErrorMessage { get; set; }
}