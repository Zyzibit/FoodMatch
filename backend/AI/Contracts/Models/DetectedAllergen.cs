namespace inzynierka.AI.Contracts.Models;

public class DetectedAllergen
{
    public string Name { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string? Source { get; set; }
    public string? Severity { get; set; }
}