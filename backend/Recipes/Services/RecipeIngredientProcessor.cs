using inzynierka.Products.Dto;
using inzynierka.Products.Model;
using inzynierka.Products.Services;
using inzynierka.Recipes.Extensions;
using inzynierka.Recipes.Model.RecipeModel;
using inzynierka.Recipes.Responses;
using inzynierka.Units.Services;

namespace inzynierka.Recipes.Services;

public class RecipeIngredientProcessor : IRecipeIngredientProcessor
{
    private readonly ILogger<RecipeIngredientProcessor> _logger;
    private readonly IProductService _productService;
    private readonly IUnitService _unitService;

    public RecipeIngredientProcessor(
        ILogger<RecipeIngredientProcessor> logger,
        IProductService productService,
        IUnitService unitService)
    {
        _logger = logger;
        _productService = productService;
        _unitService = unitService;
    }

    public async Task<List<PreviewRecipeIngredientDto>> ProcessUserProvidedIngredientsAsync(
        List<ProductDto> products,
        List<GeneratedRecipeIngredient> aiIngredients)
    {
        var previewIngredients = new List<PreviewRecipeIngredientDto>();
        var distinctProducts = products.GroupBy(p => p.Id).Select(g => g.First()).ToList();

        foreach (var product in distinctProducts)
        {
            var productName = _productService.GetProductDisplayName(product);
            var matchingIngredient = _productService.FindMatchingProduct(
                product, 
                aiIngredients.MapToProductDtoList());

            if (matchingIngredient == null)
            {
                throw new InvalidOperationException($"No matching AI ingredient found for product: {productName}");
            }

            var aiIngredient = aiIngredients.FirstOrDefault(ai => ai.Name == matchingIngredient.Name);
            if (aiIngredient == null)
            {
                throw new InvalidOperationException($"No matching AI ingredient found for product: {productName}");
            }

            try
            {
                if (!int.TryParse(product.Id, out var productId))
                {
                    continue;
                }

                var unitId = await GetUnitIdForIngredientAsync(aiIngredient.Unit);
                var units = await _unitService.GetAllUnitsAsync();
                var unitName = units.FirstOrDefault(u => u.UnitId == unitId)?.Name ?? aiIngredient.Unit;

                var previewIngredient = aiIngredient.ToPreviewIngredientDto(
                    productId,
                    productName,
                    unitId,
                    unitName,
                    aiIngredient.Quantity,
                    ProductSource.User
                );

                previewIngredients.Add(previewIngredient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process ingredient {ProductName} with unit {Unit}",
                    productName, aiIngredient.Unit);
            }
        }

        return previewIngredients;
    }

    public async Task<List<PreviewRecipeIngredientDto>> ProcessAiGeneratedIngredientsAsync(
        List<GeneratedRecipeIngredient> additionalIngredients)
    {
        var previewIngredients = new List<PreviewRecipeIngredientDto>();

        foreach (var additionalIngredient in additionalIngredients)
        {
            try
            {
                var aiGeneratedProduct = await _productService.CreateAiGeneratedProductAsync(additionalIngredient);
                var unitId = await GetUnitIdForIngredientAsync(additionalIngredient.Unit);
                var units = await _unitService.GetAllUnitsAsync();
                var unitName = units.FirstOrDefault(u => u.UnitId == unitId)?.Name ?? additionalIngredient.Unit;

                var previewIngredient = additionalIngredient.ToPreviewIngredientDto(
                    aiGeneratedProduct.Id,
                    additionalIngredient.Name,
                    unitId,
                    unitName,
                    additionalIngredient.Quantity,
                    ProductSource.AI
                );

                previewIngredients.Add(previewIngredient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process AI-generated ingredient {IngredientName}",
                    additionalIngredient.Name);
            }
        }

        return previewIngredients;
    }

    private async Task<int> GetUnitIdForIngredientAsync(string? unitName)
    {
        if (string.IsNullOrEmpty(unitName))
        {
            throw new ArgumentException("Unit name cannot be null or empty", nameof(unitName));
        }

        try
        {
            var units = await _unitService.GetAllUnitsAsync();

            if (!units.Any())
            {
                throw new InvalidOperationException("No units found in the database. Please ensure units are seeded.");
            }

            var unit = units.FirstOrDefault(u =>
                u.Name.Equals(unitName.Trim(), StringComparison.OrdinalIgnoreCase));

            if (unit != null)
            {
                return unit.UnitId;
            }

            _logger.LogWarning("Unit '{UnitName}' not found, using default 'g'. Available units: {Units}",
                unitName, string.Join(", ", units.Select(u => u.Name)));

            var defaultUnit = units.FirstOrDefault(u =>
                u.Name.Equals("gram", StringComparison.OrdinalIgnoreCase));

            if (defaultUnit != null)
            {
                return defaultUnit.UnitId;
            }

            throw new InvalidOperationException(
                $"Unit '{unitName}' not found and default unit 'gram' is missing. Available: {string.Join(", ", units.Select(u => u.Name))}");
        }
        catch (Exception ex) when (ex is not InvalidOperationException && ex is not ArgumentException)
        {
            throw new InvalidOperationException($"Failed to retrieve unit '{unitName}' from database", ex);
        }
    }


}

