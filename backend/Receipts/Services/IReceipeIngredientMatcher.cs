using inzynierka.Products.Responses;
using inzynierka.Receipts.Model.Recipe;

namespace inzynierka.Receipts.Services;

public interface IRecipeIngredientMatcher
{
    List<GeneratedRecipeIngredient> GetAdditionalIngredients(
        List<string> userProvidedIngredientNames,
        List<GeneratedRecipeIngredient> allIngredients);
    
    List<ProductInfo> GetMatchingProducts(
        List<ProductInfo> availableProducts,
        List<GeneratedRecipeIngredient> recipeIngredients);
    
    GeneratedRecipeIngredient? FindMatchingRecipeIngredient(
        ProductInfo product,
        List<GeneratedRecipeIngredient> recipeIngredients);
    
    string GetProductDisplayName(ProductInfo product);
}