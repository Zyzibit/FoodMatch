using inzynierka.Products.Extensions;
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
                
                var productDto = i.Product.ToProductDto();
                
                var productInfo = productDto.ToRecipeIngredientProduct(quantityInGrams);
                
                return new RecipeIngredientReadDto
                {
                    UnitId = i.UnitId,
                    Quantity = i.Quantity,
                    NormalizedQuantityInGrams = i.NormalizedQuantityInGrams,
                    ProductId = productInfo.ProductId,
                    ProductName = productInfo.ProductName,
                    Source = productInfo.Source,
                    UnitName = i.Unit.Name,
                    Calories = productInfo.Calories,
                    Proteins = productInfo.Proteins,
                    Carbohydrates = productInfo.Carbohydrates,
                    Fats = productInfo.Fats
                };
            }).ToList(),
            AdditionalProducts = recipe.AdditionalProducts,
            Title = recipe.Title,
            Description = recipe.Description,
            Instructions = recipe.Instructions,
            PreparationTimeMinutes = recipe.PreparationTimeMinutes,
            TotalWeightGrams = recipe.TotalWeightGrams,
            Calories = recipe.Calories,
            Proteins = recipe.Protein,
            Carbohydrates = recipe.Carbohydrates,
            Fats = recipe.Fats,
            IsPublic = recipe.IsPublic,
            CreatedAt = recipe.CreatedAt
        };
    }

    public static IEnumerable<RecipeDto> ToDtoList(this IEnumerable<Recipe> recipes)
        => recipes.Select(r => r.ToDto());
}

