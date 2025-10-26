using inzynierka.Receipts.Contracts.Models;
using inzynierka.Receipts.Model;
using inzynierka.AI.Contracts.Models;
using inzynierka.Receipts.Contracts;
using inzynierka.Receipts.Mappings;

namespace inzynierka.Receipts.Services;

public class ReceiptService
{
    private readonly ILogger<ReceiptService> _logger;
    private readonly IUnitContract _unitContract;
    private readonly IReceiptMapper _receiptMapper;

    public ReceiptService(
        ILogger<ReceiptService> logger,
        IUnitContract unitContract,
        IReceiptMapper receiptMapper)
    {
        _logger = logger;
        _unitContract = unitContract;
        _receiptMapper = receiptMapper;
    }

    /// <summary>
    /// Maps a Receipt domain model to its DTO representation.
    /// </summary>
    public ReceiptDto MapToDto(Receipt receipt)
    {
        return _receiptMapper.MapToDto(receipt);
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
}
