namespace inzynierka.AI.Contracts.Models;

public class GenerateRecipeRequest
{
    public List<string> AvailableIngredients { get; set; } = new();
    public DietaryPreferences? Preferences { get; set; }
    public string? CuisineType { get; set; }
    public int? DesiredServings { get; set; }
    public int? MaxPreparationTimeMinutes { get; set; }
    public string? AdditionalInstructions { get; set; }
}