namespace inzynierka.Products.Responses;

public class ProductResult
{
    public bool Success { get; set; }
    public ProductInfo? Product { get; set; }
    public string? ErrorMessage { get; set; }
}

