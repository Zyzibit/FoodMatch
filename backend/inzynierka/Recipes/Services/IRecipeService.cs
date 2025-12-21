using inzynierka.Recipes.Requests;
using inzynierka.Recipes.Responses;

namespace inzynierka.Recipes.Services;

public interface IRecipeService
{
    Task<CreateRecipeResult> CreateRecipeAsync(string userId, CreateRecipeRequest recipe);
    Task<RecipeDto?> GetRecipeAsync(int id);
    Task<RecipeListResult> Recipes(int limit = 50, int offset = 0);
    Task<RecipeListResult> GetUserRecipesAsync(string userId, int limit = 50, int offset = 0);
    Task<RecipeListResult> GetPublicRecipesAsync(int limit = 50, int offset = 0);
    Task<CreateRecipeResult> CopyRecipeToUserAsync(string userId, int recipeId);
    Task<CreateRecipeResult> ShareRecipeAsync(string userId, int recipeId);
    Task<bool> DeleteRecipeAsync(string userId, int recipeId);
    
    Task<GenerateRecipePreviewResult> GenerateRecipePreviewAsync(string userId, GenerateRecipeRequest request);
    Task<CreateRecipeResult> SaveGeneratedRecipeAsync(string userId, SaveGeneratedRecipeRequest request);
    
    Task<RecipeListResult> SearchRecipesAsync(SearchRecipesRequest request);
}
