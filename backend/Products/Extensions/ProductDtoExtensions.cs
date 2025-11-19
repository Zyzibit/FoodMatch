using inzynierka.Products.Dto;

namespace inzynierka.Products.Extensions;

public static class ProductDtoExtensions
{
    public static RecipeIngredientProductDto ToRecipeIngredientProduct(
        this ProductDto product, 
        decimal normalizedQuantityInGrams)
    {
        var scaleFactor = normalizedQuantityInGrams / 100m;
        
        var calories = product.Nutrition?.EstimatedCalories ?? (decimal?)product.Nutrition?.Calories ?? 0m;
        var proteins = product.Nutrition?.EstimatedProteins ?? (decimal?)product.Nutrition?.Proteins ?? 0m;
        var carbohydrates = product.Nutrition?.EstimatedCarbohydrates ?? (decimal?)product.Nutrition?.Carbohydrates ?? 0m;
        var fats = product.Nutrition?.EstimatedFats ?? (decimal?)product.Nutrition?.Fat ?? 0m;
        
        return new RecipeIngredientProductDto
        {
            ProductId = int.TryParse(product.Id, out var id) ? id : 0,
            ProductName = GetDisplayName(product),
            Source = product.Source,
            Calories = calories * scaleFactor,
            Proteins = proteins * scaleFactor,
            Carbohydrates = carbohydrates * scaleFactor,
            Fats = fats * scaleFactor
        };
    }
    
    private static string GetDisplayName(ProductDto product)
    {
        return !string.IsNullOrWhiteSpace(product.Name)
            ? product.Name
            : (!string.IsNullOrWhiteSpace(product.Brand)
                ? product.Brand
                : $"Product {product.Id}");
    }
}

public class RecipeIngredientProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public decimal Calories { get; set; }
    public decimal Proteins { get; set; }
    public decimal Carbohydrates { get; set; }
    public decimal Fats { get; set; }
}

