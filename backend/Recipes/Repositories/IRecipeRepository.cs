using inzynierka.Recipes.Model;

namespace inzynierka.Recipes.Repositories;

public interface IRecipeRepository
{
    Task<Recipe> AddRecipeAsync(Recipe recipe);
    Task<Recipe?> GetRecipeByIdAsync(int id);
    Task<Recipe> UpdateRecipeAsync(Recipe recipe);
    Task<(List<Recipe> Recipes, int TotalCount)> GetAllRecipesAsync(int limit = 50, int offset = 0);
    Task<(List<Recipe> Recipes, int TotalCount)> GetUserRecipesAsync(string userId, int limit = 50, int offset = 0);
    Task<(List<Recipe> Recipes, int TotalCount)> GetPublicRecipesAsync(int limit = 50, int offset = 0);
    Task<Recipe?> GetRecipeByIdForCopyAsync(int id);
}
