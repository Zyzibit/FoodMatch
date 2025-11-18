using inzynierka.Products.Model;

namespace inzynierka.Recipes.Responses;

public class GeneratedRecipePreviewDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public int PreparationTimeMinutes { get; set; }
    public int TotalWeightGrams { get; set; }
    public decimal Calories { get; set; }
    public decimal Proteins { get; set; }
    public decimal Carbohydrates { get; set; }
    public decimal Fats { get; set; }
    
    public List<PreviewRecipeIngredientDto> Ingredients { get; set; } = new();
    public List<string> AdditionalProducts { get; set; } = new();
}

public class PreviewRecipeIngredientDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal NormalizedQuantityInGrams { get; set; }
    public ProductSource Source { get; set; } = ProductSource.User;
    
    public decimal Calories { get; set; }
    public decimal Protein { get; set; }
    public decimal Carbohydrates { get; set; }
    public decimal Fats { get; set; }
}

public class GenerateRecipePreviewResult
{
    public bool Success { get; set; }
    public GeneratedRecipePreviewDto? Recipe { get; set; }
    public string? ErrorMessage { get; set; }
}

