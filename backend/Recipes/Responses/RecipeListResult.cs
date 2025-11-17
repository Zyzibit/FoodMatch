namespace inzynierka.Recipes.Responses;

public class RecipeListResult
{
    public bool Success { get; set; }
    public List<RecipeDto> Recipes { get; set; } = new();
    public int TotalCount { get; set; }
    public string? ErrorMessage { get; set; }
}

