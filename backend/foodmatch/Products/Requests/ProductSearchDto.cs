namespace inzynierka.Products.Dto;

public class ProductSearchDto
{
    public string? Query { get; set; }
    public List<string>? Categories { get; set; }
    public List<string>? Allergens { get; set; }
    public List<string>? Ingredients { get; set; }
    public string? Brand { get; set; }
    public int Limit { get; set; } = 50;
    public int Offset { get; set; } = 0;
}
