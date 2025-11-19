using inzynierka.Products.Dto;
using inzynierka.Products.Services;
using inzynierka.Recipes.Extensions;
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
        var ingredientDtos = recipeIngredients.MapToProductDtoList();
        return _productService.GetMatchingProducts(availableProducts, ingredientDtos);
    }
    
    public GeneratedRecipeIngredient? FindMatchingRecipeIngredient(
        ProductDto product,
        List<GeneratedRecipeIngredient> recipeIngredients)
    {
        var ingredientDtos = recipeIngredients.MapToProductDtoList();
        var matchingIngredientDto = _productService.FindMatchingProduct(product, ingredientDtos);
        
        if (matchingIngredientDto == null)
        {
            return null;
        }

        return recipeIngredients.FirstOrDefault(ri => ri.Name == matchingIngredientDto.Name);
    }
}
