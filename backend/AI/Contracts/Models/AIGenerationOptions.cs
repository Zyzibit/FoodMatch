namespace inzynierka.AI.Contracts.Models;

public class AIGenerationOptions
{
    public string? Model { get; set; }
    public double? Temperature { get; set; }
    public int? MaxTokens { get; set; }
    public string? Language { get; set; }
}