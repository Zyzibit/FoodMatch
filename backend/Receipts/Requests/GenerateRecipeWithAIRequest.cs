using inzynierka.AI.Contracts.Models;

namespace inzynierka.Receipts.Requests;

public class GenerateRecipeWithAIRequest
{
    public List<int> ProductIds { get; set; } = new();
    
    public DietaryPreferences? Preferences { get; set; }
    public string? CuisineType { get; set; }
    public int? DesiredServings { get; set; }
    public int? MaxPreparationTimeMinutes { get; set; }
    public string? AdditionalInstructions { get; set; }
}

