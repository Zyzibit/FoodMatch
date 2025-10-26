namespace inzynierka.Products.Contracts.Models;

public class ProductInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public List<string> Categories { get; set; } = new();
    public List<string> Ingredients { get; set; } = new();
    public List<string> Allergens { get; set; } = new();
    public List<string> Countries { get; set; } = new();
    public NutritionInfo? Nutrition { get; set; }
    public string? NutritionGrade { get; set; }
    public string? EcoScoreGrade { get; set; }
    public bool IsAiGenerated { get; set; }

}