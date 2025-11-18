namespace inzynierka.Recipes.Requests;

public class SaveGeneratedRecipeRequest
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
    
    public List<SaveGeneratedRecipeIngredientDto> Ingredients { get; set; } = new();
    public List<string> AdditionalProducts { get; set; } = new();
}

public class SaveGeneratedRecipeIngredientDto
{
    public int ProductId { get; set; }
    public int UnitId { get; set; }
    public decimal Quantity { get; set; }
    public decimal NormalizedQuantityInGrams { get; set; }
}

