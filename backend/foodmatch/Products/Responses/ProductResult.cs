using foodmatch.Products.Responses;

namespace foodmatch.Products.Responses;

public class ProductResult
{
    public bool Success { get; set; }
    public ProductDto? Product { get; set; }
    public string? ErrorMessage { get; set; }
}

