using inzynierka.Recipes.Requests;
using inzynierka.Recipes.Responses;

namespace inzynierka.Recipes.Services;

public interface IRecipeGeneratorService
{
    Task<GenerateRecipeResponse> GenerateRecipeAsync(GenerateRecipeRequest request);
}

