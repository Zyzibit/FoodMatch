using inzynierka.Products.Dto;
using inzynierka.Products.Services;
using inzynierka.Recipes.Model.RecipeModel;

namespace inzynierka.Recipes.Services;

public class RecipeIngredientMatcher : IRecipeIngredientMatcher
{
    private readonly ILogger<RecipeIngredientMatcher> _logger;
    private readonly IProductService _productService;

    public RecipeIngredientMatcher(ILogger<RecipeIngredientMatcher> logger, IProductService productService)
    {
        _logger = logger;
        _productService = productService;
    }
    
    public List<string> PrepareIngredientNames(List<ProductDto> products, List<string> availableIngredients)
    {
        var ingredientNames = new List<string>();

        if (products.Any())
        {
            ingredientNames.AddRange(products.Select(p => _productService.GetProductDisplayName(p)));
        }

        if (availableIngredients.Any())
        {
            ingredientNames.AddRange(availableIngredients);
        }

        return ingredientNames;
    }
    
    public List<GeneratedRecipeIngredient> GetAdditionalIngredients(
        List<string> userProvidedIngredientNames,
        List<GeneratedRecipeIngredient> allIngredients)
    {
        var additionalIngredients = allIngredients
            .Where(ai => !ai.ProductId.HasValue)
            .ToList();

        _logger.LogInformation("Found {AdditionalCount} additional ingredients not in user products (without ProductId)",
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
                .Select(p => _productService.GetProductDisplayName(p));
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
        return matchingIngredient;
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
