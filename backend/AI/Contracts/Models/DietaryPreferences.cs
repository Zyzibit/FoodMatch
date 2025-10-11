namespace inzynierka.AI.Contracts.Models;

public class DietaryPreferences
{
    public bool IsVegetarian { get; set; }
    public bool IsVegan { get; set; }
    public bool IsGlutenFree { get; set; }
    public bool IsLactoseFree { get; set; }
    public List<string> Allergies { get; set; } = new();
    public List<string> DislikedIngredients { get; set; } = new();
    public string? CuisineType { get; set; }
    public int? MaxCalories { get; set; }
}