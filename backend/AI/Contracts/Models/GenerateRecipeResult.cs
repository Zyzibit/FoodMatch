

namespace inzynierka.AI.Contracts.Models;

public class GenerateRecipeResult
{
    public bool Success { get; set; }
    public GeneratedRecipe? Recipe { get; set; }
    public string? ErrorMessage { get; set; }
}