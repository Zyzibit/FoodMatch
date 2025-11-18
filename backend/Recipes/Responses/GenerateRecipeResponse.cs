using inzynierka.Recipes.Model.RecipeModel;

namespace inzynierka.Recipes.Responses;

public class GenerateRecipeResponse
{
    public bool Success { get; set; }
    public GeneratedRecipe? Recipe { get; set; }
    public string? ErrorMessage { get; set; }
}

