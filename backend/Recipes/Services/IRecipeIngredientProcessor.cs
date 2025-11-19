using inzynierka.Products.Dto;
using inzynierka.Recipes.Model.RecipeModel;
using inzynierka.Recipes.Responses;

namespace inzynierka.Recipes.Services;

public interface IRecipeIngredientProcessor
{
    Task<List<PreviewRecipeIngredientDto>> ProcessUserProvidedIngredientsAsync(
        List<ProductDto> products,
        List<GeneratedRecipeIngredient> aiIngredients);
    
    Task<List<PreviewRecipeIngredientDto>> ProcessAiGeneratedIngredientsAsync(
        List<GeneratedRecipeIngredient> additionalIngredients);
}