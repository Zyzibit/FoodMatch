using inzynierka.Products.Dto;
using inzynierka.Products.Model;
using inzynierka.Recipes.Model.RecipeModel;
using inzynierka.Recipes.Responses;

namespace inzynierka.Recipes.Extensions;

public static class GeneratedRecipeIngredientExtensions
{
    public static ProductDto MapToProductDto(this GeneratedRecipeIngredient ingredient)
    {
        return new ProductDto
        {
            Id = ingredient.ProductId?.ToString() ?? "0",
            Name = ingredient.Name,
            Brand = string.Empty,
            Barcode = string.Empty,
            ImageUrl = string.Empty,
            Categories = new List<string>(),
            Ingredients = new List<string>(),
            Allergens = new List<string>(),
            Countries = new List<string>(),
            Nutrition = new NutritionInfoDto
            {
                Calories = (double)ingredient.EstimatedCalories,
                Proteins = (double)ingredient.EstimatedProteins,
                Carbohydrates = (double)ingredient.EstimatedCarbohydrates,
                Fat = (double)ingredient.EstimatedFats
            },
            Source = "AI"
        };
    }

    public static List<ProductDto> MapToProductDtoList(this List<GeneratedRecipeIngredient> ingredients)
    {
        return ingredients.Select(ingredient => ingredient.MapToProductDto()).ToList();
    }

    public static PreviewRecipeIngredientDto ToPreviewIngredientDto(
        this GeneratedRecipeIngredient ingredient,
        int productId,
        string productName,
        int unitId,
        string unitName,
        decimal quantity,
        ProductSource source)
    {
        return new PreviewRecipeIngredientDto
        {
            ProductId = productId,
            ProductName = productName,
            UnitId = unitId,
            UnitName = unitName,
            Quantity = quantity,
            NormalizedQuantityInGrams = ingredient.NormalizedQuantityInGrams ?? 0,
            Source = source,
            Calories = ingredient.EstimatedCalories,
            Protein = ingredient.EstimatedProteins,
            Carbohydrates = ingredient.EstimatedCarbohydrates,
            Fats = ingredient.EstimatedFats
        };
    }
}

