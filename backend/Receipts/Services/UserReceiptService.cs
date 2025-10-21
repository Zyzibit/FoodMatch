using inzynierka.Receipts.Contracts.Models;
using inzynierka.Receipts.Model;
using inzynierka.Receipts.Repositories;
using inzynierka.AI.Contracts;
using inzynierka.AI.Contracts.Models;
using inzynierka.Products.Model;
using inzynierka.Products.Repositories;
using inzynierka.Receipts.Contracts;

namespace inzynierka.Receipts.Services;

public class UserReceiptService : IReceiptService
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly ILogger<UserReceiptService> _logger;
    private readonly IAIContract _aiContract;
    private readonly IProductRepository _productRepository;
    private readonly IUnitContract _unitContract;

    public UserReceiptService(
        IReceiptRepository receiptRepository, 
        ILogger<UserReceiptService> logger,
        IAIContract aiContract,
        IProductRepository productRepository,
        IUnitContract unitContract)
    {
        _receiptRepository = receiptRepository;
        _logger = logger;
        _aiContract = aiContract;
        _productRepository = productRepository;
        _unitContract = unitContract;
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

        return MapToDto(receipt);
    }

    public async Task<ReceiptsListResult> GetAllReceiptsAsync(int limit = 50, int offset = 0)
    {
        var (receipts, total) = await _receiptRepository.GetAllReceiptsAsync(limit, offset);
        var dtoList = receipts.Select(MapToDto).ToList();
        return new ReceiptsListResult { Success = true, Receipts = dtoList, TotalCount = total };
    }

    public async Task<ReceiptsListResult> GetUserReceiptsAsync(string userId, int limit = 50, int offset = 0)
    {
        var (receipts, total) = await _receiptRepository.GetUserReceiptsAsync(userId, limit, offset);
        var dtoList = receipts.Select(MapToDto).ToList();
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
            var additionalIngredients = generatedRecipe.Ingredients
                .Where(ai => !userProvidedIngredientNames.Any(userIng => 
                    ai.Name.ToLowerInvariant().Contains(userIng) || 
                    userIng.Contains(ai.Name.ToLowerInvariant())))
                .Select(i => $"{i.Name} ({i.Quantity} {i.Unit})")
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
                AdditionalProducts = additionalIngredients
            };

            // Process ingredients with proper async handling
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
                    var unitId = await GetUnitIdForIngredientAsync(aiIngredient.Unit);
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
                    // Continue processing other ingredients
                }
            }

            var added = await _receiptRepository.AddReceiptAsync(receipt);
            
            _logger.LogInformation("AI-generated recipe created with ID: {ReceiptId}, with {Count} ingredients and {AdditionalCount} additional ingredients", 
                added.Id, receipt.Ingredients.Count, receipt.AdditionalProducts?.Count ?? 0);
            
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
        if (string.IsNullOrEmpty(productName))
        {
            _logger.LogWarning("Product name is null or empty, cannot determine quantity");
            return null;
        }

        var matchingIngredient = aiIngredients
            .FirstOrDefault(ai => productName.Contains(ai.Name, StringComparison.OrdinalIgnoreCase) ||
                                 ai.Name.Contains(productName, StringComparison.OrdinalIgnoreCase));
        
        if (matchingIngredient == null)
        {
            _logger.LogWarning("No matching AI ingredient found for product: {ProductName}", productName);
            return null;
        }

        return matchingIngredient.Quantity;
    }

    private async Task<int> GetUnitIdForIngredientAsync(string? unitName)
    {
        if (string.IsNullOrEmpty(unitName))
        {
            throw new ArgumentException("Unit name cannot be null or empty", nameof(unitName));
        }
            
        try
        {
            var units = await _unitContract.GetAllUnitsAsync();
            
            if (!units.Any())
            {
                throw new InvalidOperationException("No units found in the database. Please ensure units are seeded.");
            }
            
            var unit = units.FirstOrDefault(u => 
                u.Name.Equals(unitName, StringComparison.OrdinalIgnoreCase));
            
            if (unit != null)
            {
                return unit.UnitId;
            }
            
            _logger.LogWarning("Unit '{UnitName}' not found in database, trying default unit", unitName);
            
            var defaultUnit = units.FirstOrDefault(u => 
                u.Name.Equals("gram", StringComparison.OrdinalIgnoreCase));
            
            if (defaultUnit != null)
            {
                return defaultUnit.UnitId;
            }
            
            throw new InvalidOperationException($"Unit '{unitName}' not found in database and default unit 'gram' is also missing. Available units: {string.Join(", ", units.Select(u => u.Name))}");
        }
        catch (Exception ex) when (ex is not InvalidOperationException && ex is not ArgumentException)
        {
            _logger.LogError(ex, "Error getting unit ID for unit name: {UnitName}", unitName);
            throw new InvalidOperationException($"Failed to retrieve unit '{unitName}' from database", ex);
        }
    }
    
    private static ReceiptDto MapToDto(Receipt receipt)
    {
        return new ReceiptDto
        {
            Id = receipt.Id,
            UserId = receipt.UserId,
            IsAiGenerated = receipt.IsAiGenerated,
            Ingredients = receipt.Ingredients.Select(i => new ReceiptIngredientReadDto
            {
                ProductId = i.ProductId,
                UnitId = i.UnitId,
                Quantity = i.Quantity
            }).ToList(),
            AdditionalProducts = receipt.AdditionalProducts,
            Title = receipt.Title,
            Description = receipt.Description,
            Instructions = receipt.Instructions,
            Servings = receipt.Servings,
            PreparationTimeMinutes = receipt.PreparationTimeMinutes,
            TotalWeightGrams = receipt.TotalWeightGrams,
            CaloriesPer100G = receipt.Calories,
            ProteinPer100G = receipt.Protein,
            CarbohydratesPer100G = receipt.Carbohydrates,
            FatsPer100G = receipt.Fats,
            CreatedAt = receipt.CreatedAt
        };
    }
}
