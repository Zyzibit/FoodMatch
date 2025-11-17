using inzynierka.Recipes.Model;
using inzynierka.Recipes.Responses;

namespace inzynierka.Recipes.Extensions;

public static class RecipeExtensions
{
    public static RecipeDto ToDto(this Recipe recipe)
    {
        return new RecipeDto
        {
            Id = recipe.Id,
            UserId = recipe.UserId,
            Source = recipe.Source.ToString(),
            Ingredients = recipe.Ingredients.Select(i =>
            {
                var quantityInGrams = i.NormalizedQuantityInGrams ?? 100m;
                var scaleFactor = quantityInGrams / 100m;
                
                return new RecipeIngredientReadDto
                {
                    ProductId = i.ProductId,
                    UnitId = i.UnitId,
                    Quantity = i.Quantity,
                    NormalizedQuantityInGrams = i.NormalizedQuantityInGrams,
                    ProductName = i.Product.ProductName ?? "",
                    Source = i.Product.Source.ToString(),
                    Calories = (i.Product.estimatedCalories ?? (decimal?)(i.Product.EnergyKcal100g ?? 0) ?? 0) * scaleFactor,
                    Proteins = (i.Product.estimatedProteins ?? (decimal?)(i.Product.Proteins100g ?? 0) ?? 0) * scaleFactor,
                    Carbohydrates = (i.Product.estimatedCarbohydrates ?? (decimal?)(i.Product.Carbohydrates100g ?? 0) ?? 0) * scaleFactor,
                    Fats = (i.Product.estimatedFats ?? (decimal?)(i.Product.Fat100g ?? 0) ?? 0) * scaleFactor
                };
            }).ToList(),
            AdditionalProducts = recipe.AdditionalProducts,
            Title = recipe.Title,
            Description = recipe.Description,
            Instructions = recipe.Instructions,
            Servings = recipe.Servings,
            PreparationTimeMinutes = recipe.PreparationTimeMinutes,
            TotalWeightGrams = recipe.TotalWeightGrams,
            Calories = recipe.Calories,
            Protein = recipe.Protein,
            Carbohydrates = recipe.Carbohydrates,
            Fats = recipe.Fats,
            CreatedAt = recipe.CreatedAt
        };
    }

    public static IEnumerable<RecipeDto> ToDtoList(this IEnumerable<Recipe> recipes)
        => recipes.Select(r => r.ToDto());
}

