using foodmatch.Recipes.Model.RecipeModel;
using foodmatch.Recipes.Responses;

namespace foodmatch.Recipes.Extensions;

public static class GeneratedRecipeExtensions
{
    public static GeneratedRecipePreviewDto ToPreviewDto(
        this GeneratedRecipe recipe,
        List<PreviewRecipeIngredientDto> ingredients,
        List<string> additionalProducts)
    {
        return new GeneratedRecipePreviewDto
        {
            Title = recipe.Title,
            Description = recipe.Description,
            Instructions = recipe.Instructions,
            PreparationTimeMinutes = recipe.PreparationTimeMinutes,
            TotalWeightGrams = recipe.TotalWeightGrams,
            Calories = recipe.EstimatedCalories,
            Proteins = recipe.EstimatedProtein,
            Carbohydrates = recipe.EstimatedCarbohydrates,
            Fats = recipe.EstimatedFats,
            Ingredients = ingredients,
            AdditionalProducts = additionalProducts
        };
    }
}

