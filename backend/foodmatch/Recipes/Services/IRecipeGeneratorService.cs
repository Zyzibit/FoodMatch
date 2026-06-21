using foodmatch.Recipes.Requests;
using foodmatch.Recipes.Responses;

namespace foodmatch.Recipes.Services;

public interface IRecipeGeneratorService
{
    Task<GenerateRecipeResponse> GenerateRecipeAsync(GenerateRecipeRequest request);
}

