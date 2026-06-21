namespace inzynierka.Recipes.Responses;

public class CreateRecipeResult
{
    public bool Success { get; set; }
    public int? RecipeId { get; set; }
    public string? ErrorMessage { get; set; }
}

