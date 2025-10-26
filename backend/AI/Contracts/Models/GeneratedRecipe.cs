namespace inzynierka.AI.Contracts.Models;

public class GeneratedRecipe
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<GeneratedRecipeIngredient> Ingredients { get; set; } = new();
    public string Instructions { get; set; } = string.Empty;
    public int Servings { get; set; }
    public int PreparationTimeMinutes { get; set; }
    public int TotalWeightGrams { get; set; }
    public decimal EstimatedCalories { get; set; }
    public decimal EstimatedProtein { get; set; }
    public decimal EstimatedCarbohydrates { get; set; }
    public decimal EstimatedFats { get; set; }
}

public class GeneratedRecipeIngredient
{
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal EstimatedCalories { get; set; }
    public decimal EstimatedProteins { get; set; }
    public decimal EstimatedCarbohydrates { get; set; }
    public decimal EstimatedFats { get; set; }
}
