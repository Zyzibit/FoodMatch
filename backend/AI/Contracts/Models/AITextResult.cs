namespace inzynierka.AI.Contracts.Models;

public class AITextResult
{
    public bool Success { get; set; }
    public string? Response { get; set; }
    public string? ModelUsed { get; set; }
    public int? TokensUsed { get; set; }
    public string? ErrorMessage { get; set; }
}