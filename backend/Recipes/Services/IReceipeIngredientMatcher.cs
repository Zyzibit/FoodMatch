using inzynierka.Products.Dto;
using inzynierka.Recipes.Model.RecipeModel;

namespace inzynierka.Recipes.Services;

public interface IRecipeIngredientMatcher
{
    List<string> PrepareIngredientNames(List<ProductDto> products, List<string> availableIngredients);
    
    List<GeneratedRecipeIngredient> GetAdditionalIngredients(
        List<string> userProvidedIngredientNames,
        List<GeneratedRecipeIngredient> allIngredients);
    
    List<ProductDto> GetMatchingProducts(
        List<ProductDto> availableProducts,
        List<GeneratedRecipeIngredient> recipeIngredients);
    
    GeneratedRecipeIngredient? FindMatchingRecipeIngredient(
        ProductDto product,
        List<GeneratedRecipeIngredient> recipeIngredients);
}