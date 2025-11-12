using inzynierka.Products.Responses;
using inzynierka.Receipts.Model.Recipe;

namespace inzynierka.Receipts.Services;

public interface IRecipeIngredientMatcher
{
    List<GeneratedRecipeIngredient> GetAdditionalIngredients(
        List<string> userProvidedIngredientNames,
        List<GeneratedRecipeIngredient> allIngredients);
    
    List<ProductDto> GetMatchingProducts(
        List<ProductDto> availableProducts,
        List<GeneratedRecipeIngredient> recipeIngredients);
    
    GeneratedRecipeIngredient? FindMatchingRecipeIngredient(
        ProductDto product,
        List<GeneratedRecipeIngredient> recipeIngredients);
    
    string GetProductDisplayName(ProductDto product);
}