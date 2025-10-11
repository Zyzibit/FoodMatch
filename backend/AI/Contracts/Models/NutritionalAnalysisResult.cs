namespace inzynierka.AI.Contracts.Models;

public class NutritionalAnalysisResult
{
    public bool Success { get; set; }
    public string? Analysis { get; set; }
    public NutritionScore? Score { get; set; }
    public List<string>? Recommendations { get; set; }
    public string? ErrorMessage { get; set; }
}