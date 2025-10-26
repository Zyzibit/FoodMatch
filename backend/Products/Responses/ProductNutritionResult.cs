namespace inzynierka.Products.Responses;

public class ProductNutritionResult
{
    public bool Success { get; set; }
    public NutritionInfo? Nutrition { get; set; }
    public string? ErrorMessage { get; set; }
}

