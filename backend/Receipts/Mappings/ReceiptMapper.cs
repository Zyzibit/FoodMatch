using inzynierka.Receipts.Responses;
using inzynierka.Receipts.Model;

namespace inzynierka.Receipts.Mappings;

public class ReceiptMapper : IReceiptMapper
{
    public ReceiptDto MapToDto(Receipt receipt)
    {
        return new ReceiptDto
        {
            Id = receipt.Id,
            UserId = receipt.UserId,
            IsAiGenerated = receipt.IsAiGenerated,
            Ingredients = receipt.Ingredients.Select(i =>
            {
                var quantityInGrams = i.NormalizedQuantityInGrams ?? 100m;
                var scaleFactor = quantityInGrams / 100m;
                
                return new ReceiptIngredientReadDto
                {
                    ProductId = i.ProductId,
                    UnitId = i.UnitId,
                    Quantity = i.Quantity,
                    NormalizedQuantityInGrams = i.NormalizedQuantityInGrams,
                    IsAiGenerated = i.Product.IsAiGenerated,
                    ProductName = i.Product.ProductName ?? "",
                    EstimatedCalories = (i.Product.estimatedCalories ?? 0) * scaleFactor,
                    EstimatedProteins = (i.Product.estimatedProteins ?? 0) * scaleFactor,
                    EstimatedCarbohydrates = (i.Product.estimatedCarbohydrates ?? 0) * scaleFactor,
                    EstimatedFats = (i.Product.estimatedFats ?? 0) * scaleFactor
                };
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

    public IEnumerable<ReceiptDto> MapToDtoList(IEnumerable<Receipt> receipts)
    {
        return receipts.Select(MapToDto);
    }
}

