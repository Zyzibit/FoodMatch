using inzynierka.Recipes.Model;

namespace inzynierka.Recipes.Repositories;

public interface IRecipeRepository
{
    Task<Recipe> AddRecipeAsync(Recipe recipe);
    Task<Recipe?> GetRecipeByIdAsync(int id);
    Task<(List<Recipe> Recipes, int TotalCount)> GetAllRecipesAsync(int limit = 50, int offset = 0);
    Task<(List<Recipe> Recipes, int TotalCount)> GetUserRecipesAsync(string userId, int limit = 50, int offset = 0);
}
