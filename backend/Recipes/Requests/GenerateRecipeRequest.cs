using inzynierka.Recipes.Model.RecipeModel;

namespace inzynierka.Recipes.Requests;

public class GenerateRecipeRequest
{
    public List<int> ProductIds { get; set; } = new();
    public List<string> AvailableIngredients { get; set; } = new();
    public DietaryPreferences? Preferences { get; set; }
    public string? CuisineType { get; set; }
    public int? MaxPreparationTimeMinutes { get; set; }
    public string? AdditionalInstructions { get; set; }
    public string? MealType { get; set; }
}

