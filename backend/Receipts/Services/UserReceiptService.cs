using inzynierka.Receipts.Contracts.Models;
using inzynierka.Receipts.Model;
using inzynierka.Receipts.Repositories;
using inzynierka.AI.Contracts;
using inzynierka.AI.Contracts.Models;
using inzynierka.Products.Model;
using inzynierka.Products.Repositories;
using inzynierka.Products.Contracts;
using inzynierka.Receipts.Contracts;

namespace inzynierka.Receipts.Services;

public class UserReceiptService : IReceiptService
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly ILogger<UserReceiptService> _logger;
    private readonly IAIContract _aiContract;
    private readonly IProductRepository _productRepository;
    private readonly IProductContract _productContract;
    private readonly IUnitContract _unitContract;
    private readonly ReceiptService _receiptService;

    public UserReceiptService(
        IReceiptRepository receiptRepository, 
        ILogger<UserReceiptService> logger,
        IAIContract aiContract,
        IProductRepository productRepository,
        IProductContract productContract,
        IUnitContract unitContract,
        ReceiptService receiptService)
    {
        _receiptRepository = receiptRepository;
        _logger = logger;
        _aiContract = aiContract;
        _productRepository = productRepository;
        _productContract = productContract;
        _unitContract = unitContract;
        _receiptService = receiptService;
    }

    public async Task<CreateReceiptResult> CreateReceiptAsync(string userId, CreateReceiptRequest request)
    {
        try
        {
            var receipt = new Receipt
            {
                UserId = userId,
                IsAiGenerated = false,
                Ingredients = request.Ingredients.Select(i => new ReceiptIngredient
                {
                    ProductId = i.ProductId,
                    UnitId = i.UnitId,
                    Quantity = i.Quantity
                }).ToList(),
                AdditionalProducts = request.AdditionalProducts,
                Title = request.Title,
                Description = request.Description,
                Instructions = request.Instructions,
                Servings = request.Servings,
                PreparationTimeMinutes = request.PreparationTimeMinutes,
                TotalWeightGrams = request.TotalWeightGrams,
                Calories = request.CaloriesPer100G,
                Protein = request.ProteinPer100G,
                Carbohydrates = request.CarbohydratesPer100G,
                Fats = request.FatsPer100G,
                CreatedAt = DateTime.UtcNow
            };

            var added = await _receiptRepository.AddReceiptAsync(receipt);
            return new CreateReceiptResult { Success = true, ReceiptId = added.Id };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating receipt");
            return new CreateReceiptResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<ReceiptDto?> GetReceiptAsync(int id)
    {
        var receipt = await _receiptRepository.GetReceiptByIdAsync(id);
        if (receipt == null) return null;

        return _receiptService.MapToDto(receipt);
    }

    public async Task<ReceiptsListResult> GetAllReceiptsAsync(int limit = 50, int offset = 0)
    {
        var (receipts, total) = await _receiptRepository.GetAllReceiptsAsync(limit, offset);
        var dtoList = receipts.Select(_receiptService.MapToDto).ToList();
        return new ReceiptsListResult { Success = true, Receipts = dtoList, TotalCount = total };
    }

    public async Task<ReceiptsListResult> GetUserReceiptsAsync(string userId, int limit = 50, int offset = 0)
    {
        var (receipts, total) = await _receiptRepository.GetUserReceiptsAsync(userId, limit, offset);
        var dtoList = receipts.Select(_receiptService.MapToDto).ToList();
        return new ReceiptsListResult { Success = true, Receipts = dtoList, TotalCount = total };
    }

    public async Task<CreateReceiptResult> GenerateRecipeWithAIAsync(string userId, GenerateRecipeWithAIRequest request)
    {
        try
        {
            var products = await _productRepository.GetProductsByIdsAsync(request.ProductIds);
            var productsList = products.ToList();
            
            // if (!productsList.Any())
            // {
            //     return new CreateReceiptResult 
            //     { 
            //         Success = false, 
            //         ErrorMessage = "No products found with the provided IDs" 
            //     };
            // }
            
            var foundProductIds = productsList.Select(p => p.Id).ToList();
            var missingProductIds = request.ProductIds.Except(foundProductIds).ToList();
                    
            if (missingProductIds.Any())
            {
                _logger.LogWarning("Missing products with IDs: {MissingIds}", string.Join(", ", missingProductIds));
            }
            
            var ingredientNames = productsList
                .Select(p => p.ProductName ?? p.Brands ?? $"Produkt {p.Id}")
                .ToList();
            
            var aiRequest = new GenerateRecipeRequest
            {
                AvailableIngredients = ingredientNames,
                Preferences = request.Preferences,
                CuisineType = request.CuisineType,
                DesiredServings = request.DesiredServings,
                MaxPreparationTimeMinutes = request.MaxPreparationTimeMinutes,
                AdditionalInstructions = request.AdditionalInstructions
            };

            var aiResult = await _aiContract.GenerateRecipeAsync(aiRequest);

            if (!aiResult.Success || aiResult.Recipe == null)
            {
                return new CreateReceiptResult 
                { 
                    Success = false, 
                    ErrorMessage = aiResult.ErrorMessage ?? "Failed to generate recipe with AI" 
                };
            }

            var generatedRecipe = aiResult.Recipe;

            var userProvidedIngredientNames = ingredientNames.Select(n => n.ToLowerInvariant()).ToList();
            var additionalIngredientsData = generatedRecipe.Ingredients
                .Where(ai => !userProvidedIngredientNames.Any(userIng => 
                    ai.Name.ToLowerInvariant().Contains(userIng) || 
                    userIng.Contains(ai.Name.ToLowerInvariant())))
                .ToList();

            var usedProducts = productsList
                .Where(p => generatedRecipe.Ingredients.Any(ai => 
                    (p.ProductName != null && ai.Name.ToLowerInvariant().Contains(p.ProductName.ToLowerInvariant())) ||
                    (p.ProductName != null && p.ProductName.ToLowerInvariant().Contains(ai.Name.ToLowerInvariant())) ||
                    (p.Brands != null && ai.Name.ToLowerInvariant().Contains(p.Brands.ToLowerInvariant())) ||
                    (p.Brands != null && p.Brands.ToLowerInvariant().Contains(ai.Name.ToLowerInvariant()))))
                .ToList();

            if (usedProducts.Count < productsList.Count)
            {
                var unusedProducts = productsList.Except(usedProducts).Select(p => p.ProductName ?? p.Id.ToString());
                _logger.LogInformation("AI used {UsedCount}/{TotalCount} products. Unused: {UnusedProducts}", 
                    usedProducts.Count, productsList.Count, string.Join(", ", unusedProducts));
            }

            var receipt = new Receipt
            {
                UserId = userId,
                IsAiGenerated = true,
                Title = generatedRecipe.Title,
                Description = generatedRecipe.Description,
                Instructions = generatedRecipe.Instructions,
                Servings = generatedRecipe.Servings,
                PreparationTimeMinutes = generatedRecipe.PreparationTimeMinutes,
                TotalWeightGrams = generatedRecipe.TotalWeightGrams,
                Calories = generatedRecipe.EstimatedCalories,
                Protein = generatedRecipe.EstimatedProtein,
                Carbohydrates = generatedRecipe.EstimatedCarbohydrates,
                Fats = generatedRecipe.EstimatedFats,
                CreatedAt = DateTime.UtcNow,
                Ingredients = new List<ReceiptIngredient>(),
                AdditionalProducts = new List<string>()
            };

            foreach (var product in usedProducts)
            {
                var aiIngredient = generatedRecipe.Ingredients.FirstOrDefault(ai => 
                    (product.ProductName != null && ai.Name.ToLowerInvariant().Contains(product.ProductName.ToLowerInvariant())) ||
                    (product.ProductName != null && product.ProductName.ToLowerInvariant().Contains(ai.Name.ToLowerInvariant())) ||
                    (product.Brands != null && ai.Name.ToLowerInvariant().Contains(product.Brands.ToLowerInvariant())) ||
                    (product.Brands != null && product.Brands.ToLowerInvariant().Contains(ai.Name.ToLowerInvariant())));

                if (aiIngredient == null)
                {
                    _logger.LogWarning("No matching AI ingredient found for product: {ProductName}", product.ProductName);
                    continue;
                }

                var quantity = GetQuantityForIngredient(product.ProductName, generatedRecipe.Ingredients);
                if (!quantity.HasValue)
                {
                    _logger.LogWarning("Could not determine quantity for product: {ProductName}", product.ProductName);
                    continue;
                }

                try
                {
                    var unitId = await _receiptService.GetUnitIdForIngredientAsync(aiIngredient.Unit);
                    receipt.Ingredients.Add(new ReceiptIngredient
                    {
                        ProductId = product.Id,
                        UnitId = unitId,
                        Quantity = quantity.Value
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to add ingredient {ProductName} with unit {Unit}", product.ProductName, aiIngredient.Unit);
                }
            }

            foreach (var additionalIngredient in additionalIngredientsData)
            {
                try
                {
                    var aiGeneratedProduct = await _receiptService.CreateAiGeneratedProductAsync(additionalIngredient);
                    var unitId = await _receiptService.GetUnitIdForIngredientAsync(additionalIngredient.Unit);
                    
                    receipt.Ingredients.Add(new ReceiptIngredient
                    {
                        ProductId = aiGeneratedProduct.Id,
                        UnitId = unitId,
                        Quantity = additionalIngredient.Quantity
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to add AI-generated ingredient {IngredientName}", additionalIngredient.Name);
                    receipt.AdditionalProducts.Add($"{additionalIngredient.Name} ({additionalIngredient.Quantity} {additionalIngredient.Unit})");
                }
            }

            var added = await _receiptRepository.AddReceiptAsync(receipt);
            
            _logger.LogInformation("AI-generated recipe created with ID: {ReceiptId}, with {Count} ingredients and {AdditionalCount} AI-generated products", 
                added.Id, receipt.Ingredients.Count, additionalIngredientsData.Count);
            
            return new CreateReceiptResult 
            { 
                Success = true, 
                ReceiptId = added.Id 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recipe with AI");
            return new CreateReceiptResult 
            { 
                Success = false, 
                ErrorMessage = ex.Message 
            };
        }
    }
    
    private decimal? GetQuantityForIngredient(string? productName, List<GeneratedRecipeIngredient> aiIngredients)
    {
        return _receiptService.GetQuantityForIngredient(productName, aiIngredients);
    }

}
