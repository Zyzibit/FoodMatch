namespace inzynierka.AI.Contracts.Models;

public class AllergenDetectionResult
{
    public bool Success { get; set; }
    public List<DetectedAllergen> DetectedAllergens { get; set; } = new();
    public string? ErrorMessage { get; set; }
}