namespace inzynierka.AI.Contracts.Models;

public class RecipeRecommendationResult
{
    public bool Success { get; set; }
    public List<RecipeRecommendation> Recommendations { get; set; } = new();
    public string? ErrorMessage { get; set; }
}