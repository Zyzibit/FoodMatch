using inzynierka.Receipts.Contracts.Models;
using inzynierka.Receipts.Model;
using inzynierka.Receipts.Repositories;
using inzynierka.AI.Contracts;
using inzynierka.AI.Contracts.Models;
using inzynierka.Products.Model;
using inzynierka.Products.Repositories;

namespace inzynierka.Receipts.Services;

public class UserReceiptService : IReceiptService
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly ILogger<UserReceiptService> _logger;
    private readonly IAIContract _aiContract;
    private readonly IProductRepository _productRepository;

    public UserReceiptService(
        IReceiptRepository receiptRepository, 
        ILogger<UserReceiptService> logger,
        IAIContract aiContract,
        IProductRepository productRepository)
    {
        _receiptRepository = receiptRepository;
        _logger = logger;
        _aiContract = aiContract;
        _productRepository = productRepository;
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
                Calories = request.Calories,
                Protein = request.Protein,
                Carbohydrates = request.Carbohydrates,
                Fats = request.Fats,
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
            // Pobierz produkty z bazy danych
            var products = await _productRepository.GetProductsByIdsAsync(request.ProductIds);
            var productsList = products.ToList();
            
            if (!productsList.Any())
            {
                return new CreateReceiptResult 
                { 
                    Success = false, 
                    ErrorMessage = "Nie znaleziono żadnych produktów o podanych ID" 
                };
            }
            
            var foundProductIds = productsList.Select(p => p.Id).ToList();
            var missingProductIds = request.ProductIds.Except(foundProductIds).ToList();
            
            if (missingProductIds.Any())
            {
                _logger.LogWarning("Missing products with IDs: {MissingIds}", string.Join(", ", missingProductIds));
            }
            
            // Przygotuj nazwy produktów dla AI
            var ingredientNames = productsList
                .Select(p => p.ProductName ?? p.Brands ?? $"Produkt {p.Id}")
                .ToList();
            
            // Wywołanie modułu AI do wygenerowania przepisu
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
                Calories = generatedRecipe.EstimatedCalories,
                Protein = generatedRecipe.EstimatedProtein,
                Carbohydrates = generatedRecipe.EstimatedCarbohydrates,
                Fats = generatedRecipe.EstimatedFats,
                CreatedAt = DateTime.UtcNow,
                Ingredients = usedProducts
                    .Select(p => new
                    {
                        Product = p,
                        Quantity = GetQuantityForIngredient(p.ProductName, generatedRecipe.Ingredients),
                        UnitId = GetUnitIdForIngredient(generatedRecipe.Ingredients.FirstOrDefault(ai => 
                            (p.ProductName != null && ai.Name.ToLowerInvariant().Contains(p.ProductName.ToLowerInvariant())) ||
                            (p.Brands != null && ai.Name.ToLowerInvariant().Contains(p.Brands.ToLowerInvariant())))?.Unit)
                    })
                    .Where(x => x.Quantity.HasValue)
                    .Select(x => new ReceiptIngredient
                    {
                        ProductId = x.Product.Id,
                        UnitId = x.UnitId,
                        Quantity = x.Quantity!.Value
                    })
                    .ToList(),
                AdditionalProducts = additionalIngredients
            };

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

        // Spróbuj znaleźć pasujący składnik z AI
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

    private int GetUnitIdForIngredient(string? unitName)
    {
        if (string.IsNullOrEmpty(unitName))
            return 1;
            
        return unitName.ToLowerInvariant() switch
        {
            "gram" => 1,
            "kilogram" => 2,
            "mililitr" => 3,
            "litr" => 4,
            "sztuka" => 5,
            "łyżka" or "lyzka" => 6,
            "łyżeczka" or "lyzeczka" => 7,
            "szklanka" => 8,
            "opakowanie" => 9,
            "garść" or "garsc" => 10,
            "plasterek" => 11,
            "kostka" => 12,
            _ => 1 
        };
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
            Calories = receipt.Calories,
            Protein = receipt.Protein,
            Carbohydrates = receipt.Carbohydrates,
            Fats = receipt.Fats,
            CreatedAt = receipt.CreatedAt
        };
    }
}
