namespace inzynierka.AI.Contracts.Models;

public class AIJsonResult
{
    public bool Success { get; set; }
    public string? JsonResponse { get; set; }
    public bool IsValidJson { get; set; }
    public string? ErrorMessage { get; set; }
}