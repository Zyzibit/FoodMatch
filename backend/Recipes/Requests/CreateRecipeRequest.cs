namespace inzynierka.Recipes.Requests;

public class CreateRecipeRequest
{
    public List<RecipeIngredientDto> Ingredients { get; set; } = new List<RecipeIngredientDto>();
    public List<string>? AdditionalProducts { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public int Servings { get; set; }
    public int PreparationTimeMinutes { get; set; }
    public int TotalWeightGrams { get; set; }
    public decimal Calories { get; set; }
    public decimal Proteins { get; set; }
    public decimal Carbohydrates { get; set; }
    public decimal Fats { get; set; }
}


