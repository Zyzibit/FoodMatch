using foodmatch.Products.Responses;

namespace foodmatch.Products.Responses;

public class ProductNutritionResult
{
    public bool Success { get; set; }
    public NutritionInfoDto? Nutrition { get; set; }
    public string? ErrorMessage { get; set; }
}

