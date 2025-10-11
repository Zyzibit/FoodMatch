namespace inzynierka.AI.Contracts.Models;

public class ProductAnalysisResult
{
    public bool Success { get; set; }
    public string? Analysis { get; set; }
    public double ConfidenceScore { get; set; }
    public List<string>? Tags { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public string? ErrorMessage { get; set; }
}