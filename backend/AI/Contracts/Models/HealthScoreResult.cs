namespace inzynierka.AI.Contracts.Models;

public class HealthScoreResult
{
    public bool Success { get; set; }
    public double Score { get; set; }
    public string? Grade { get; set; }
    public List<string>? PositiveAspects { get; set; }
    public List<string>? NegativeAspects { get; set; }
    public string? ErrorMessage { get; set; }
}