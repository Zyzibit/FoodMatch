namespace inzynierka.AI.Contracts.Models;

public class GeneratedRecipe
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<GeneratedRecipeIngredient> Ingredients { get; set; } = new();
    public string Instructions { get; set; } = string.Empty;
    public int Servings { get; set; }
    public int PreparationTimeMinutes { get; set; }
    public int EstimatedCalories { get; set; }
    public int EstimatedProtein { get; set; }
    public int EstimatedCarbohydrates { get; set; }
    public int EstimatedFats { get; set; }
}

public class GeneratedRecipeIngredient
{
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
}

