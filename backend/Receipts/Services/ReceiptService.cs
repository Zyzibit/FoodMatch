using inzynierka.Receipts.Contracts.Models;
using inzynierka.Receipts.Model;
using inzynierka.AI.Contracts.Models;
using inzynierka.Products.Model;
using inzynierka.Products.Repositories;
using inzynierka.Products.Contracts;
using inzynierka.Receipts.Contracts;

namespace inzynierka.Receipts.Services;

public class ReceiptService
{
    private readonly ILogger<ReceiptService> _logger;
    private readonly IProductContract _productContract;
    private readonly IProductRepository _productRepository;
    private readonly IUnitContract _unitContract;

    public ReceiptService(
        ILogger<ReceiptService> logger,
        IProductContract productContract,
        IProductRepository productRepository,
        IUnitContract unitContract)
    {
        _logger = logger;
        _productContract = productContract;
        _productRepository = productRepository;
        _unitContract = unitContract;
    }

    public decimal? GetQuantityForIngredient(string? productName, List<GeneratedRecipeIngredient> aiIngredients)
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

    public async Task<int> GetUnitIdForIngredientAsync(string? unitName)
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
    
    public ReceiptDto MapToDto(Receipt receipt)
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

    public async Task<Product> CreateAiGeneratedProductAsync(GeneratedRecipeIngredient ingredient)
    {
        if (string.IsNullOrEmpty(ingredient.Name))
        {
            throw new ArgumentException("Ingredient name cannot be null or empty", nameof(ingredient));
        }

        try
        {
            var result = await _productContract.AddAiProductAsync(ingredient.Name);
            
            if (!result.Success || result.Product == null)
            {
                throw new InvalidOperationException($"Failed to create AI product: {result.ErrorMessage}");
            }

            var productId = int.Parse(result.Product.Id);
            var createdProduct = await _productRepository.GetProductByIdAsync(productId);

            if (createdProduct == null)
            {
                throw new InvalidOperationException($"Created product with ID {productId} could not be retrieved");
            }

            _logger.LogInformation("Created AI-generated product: {ProductName} with ID: {ProductId}", ingredient.Name, createdProduct.Id);
            return createdProduct;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating AI-generated product: {ProductName}", ingredient.Name);
            throw;
        }
    }
}

