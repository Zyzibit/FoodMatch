using foodmatch.Recipes.Model.RecipeModel;

namespace foodmatch.Recipes.Responses;

public class GenerateRecipeResponse
{
    public bool Success { get; set; }
    public GeneratedRecipe? Recipe { get; set; }
    public string? ErrorMessage { get; set; }
}

