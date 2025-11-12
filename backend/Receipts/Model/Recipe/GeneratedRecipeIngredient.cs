namespace inzynierka.Receipts.Model.Recipe;

public class GeneratedRecipeIngredient
{
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal? NormalizedQuantityInGrams { get; set; }
    public decimal EstimatedCalories { get; set; }
    public decimal EstimatedProteins { get; set; }
    public decimal EstimatedCarbohydrates { get; set; }
    public decimal EstimatedFats { get; set; }
}

