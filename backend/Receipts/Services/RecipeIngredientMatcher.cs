using inzynierka.Products.Dto;
using inzynierka.Products.Responses;
using inzynierka.Receipts.Model.Recipe;

namespace inzynierka.Receipts.Services;

public class RecipeIngredientMatcher : IRecipeIngredientMatcher
{
    private readonly ILogger<RecipeIngredientMatcher> _logger;

    public RecipeIngredientMatcher(ILogger<RecipeIngredientMatcher> logger)
    {
        _logger = logger;
    }
    
    public List<GeneratedRecipeIngredient> GetAdditionalIngredients(
        List<string> userProvidedIngredientNames,
        List<GeneratedRecipeIngredient> allIngredients)
    {
        var additionalIngredients = allIngredients
            .Where(ai => !userProvidedIngredientNames.Any(userIng =>
                ai.Name.ToLowerInvariant().Contains(userIng) ||
                userIng.Contains(ai.Name.ToLowerInvariant())))
            .ToList();

        _logger.LogInformation("Found {AdditionalCount} additional ingredients not in user products",
            additionalIngredients.Count);

        return additionalIngredients;
    }


    public List<ProductDto> GetMatchingProducts(
        
        List<ProductDto> availableProducts,
        List<GeneratedRecipeIngredient> recipeIngredients)
    {
        var matchingProducts = availableProducts
            .Where(p => recipeIngredients.Any(ai =>
                IsProductMatchingIngredient(p, ai)))
            .ToList();

        if (matchingProducts.Count < availableProducts.Count)
        {
            var unusedProducts = availableProducts.Except(matchingProducts)
                .Select(GetProductDisplayName);
            _logger.LogInformation(
                "Matched {MatchedCount}/{TotalCount} products. Unused: {UnusedProducts}",
                matchingProducts.Count, availableProducts.Count, string.Join(", ", unusedProducts));
        }

        return matchingProducts;
    }
    
    public GeneratedRecipeIngredient? FindMatchingRecipeIngredient(
        ProductDto product,
        List<GeneratedRecipeIngredient> recipeIngredients)
    {
        var matchingIngredient = recipeIngredients.FirstOrDefault(ai =>
            IsProductMatchingIngredient(product, ai));

        if (matchingIngredient == null)
        {
            _logger.LogWarning("No matching recipe ingredient found for product: {ProductName}",
                GetProductDisplayName(product));
        }

        return matchingIngredient;
    }
    
    public string GetProductDisplayName(ProductDto product)
    {
        return !string.IsNullOrWhiteSpace(product.Name)
            ? product.Name
            : (!string.IsNullOrWhiteSpace(product.Brand)
                ? product.Brand
                : $"Product {product.Id}");
    }
    
    private bool IsProductMatchingIngredient(ProductDto product, GeneratedRecipeIngredient ingredient)
    {
        var ingredientNameLower = ingredient.Name.ToLowerInvariant();

        if (!string.IsNullOrWhiteSpace(product.Name))
        {
            var productNameLower = product.Name.ToLowerInvariant();
            if (ingredientNameLower.Contains(productNameLower) ||
                productNameLower.Contains(ingredientNameLower))
            {
                return true;
            }
        }

        if (!string.IsNullOrWhiteSpace(product.Brand))
        {
            var brandLower = product.Brand.ToLowerInvariant();
            if (ingredientNameLower.Contains(brandLower) ||
                brandLower.Contains(ingredientNameLower))
            {
                return true;
            }
        }

        return false;
    }
}
