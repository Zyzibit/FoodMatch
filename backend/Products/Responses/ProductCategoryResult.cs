namespace inzynierka.Products.Responses;

public class ProductCategoryResult
{
    public bool Success { get; set; }
    public List<ProductDto> Products { get; set; } = new();
    public int TotalCount { get; set; }
    public string? ErrorMessage { get; set; }
}
