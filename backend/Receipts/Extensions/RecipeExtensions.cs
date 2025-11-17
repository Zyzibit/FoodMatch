using inzynierka.Receipts.Model;
using inzynierka.Receipts.Responses;

namespace inzynierka.Receipts.Extensions;

public static class RecipeExtensions
{
    public static ReceiptDto ToDto(this Receipt receipt)
    {
        return new ReceiptDto
        {
            Id = receipt.Id,
            UserId = receipt.UserId,
            Source = receipt.Source.ToString(),
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
                    ProductName = i.Product.ProductName ?? "",
                    Source = i.Product.Source.ToString(),
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
            Calories = receipt.Calories,
            Protein = receipt.Protein,
            Carbohydrates = receipt.Carbohydrates,
            Fats = receipt.Fats,
            CreatedAt = receipt.CreatedAt
        };
    }

    public static IEnumerable<ReceiptDto> ToDtoList(this IEnumerable<Receipt> receipts)
        => receipts.Select(r => r.ToDto());
}

