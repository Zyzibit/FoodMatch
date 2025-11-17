using inzynierka.Receipts.Extensions.Model.Recipe;

namespace inzynierka.Receipts.Extensions.Responses;

public class GenerateRecipeResponse
{
    public bool Success { get; set; }
    public GeneratedRecipe? Recipe { get; set; }
    public string? ErrorMessage { get; set; }
}

