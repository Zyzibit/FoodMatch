using inzynierka.Receipts.Model.Recipe;

namespace inzynierka.Receipts.Responses;

public class GenerateRecipeResponse
{
    public bool Success { get; set; }
    public GeneratedRecipe? Recipe { get; set; }
    public string? ErrorMessage { get; set; }
}

