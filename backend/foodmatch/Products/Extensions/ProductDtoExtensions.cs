using inzynierka.Products.Dto;
using inzynierka.Products.Model;

namespace inzynierka.Products.Extensions;

public static class ProductDtoExtensions
{
    public static ProductDto ToProductDto(this Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        return new ProductDto
        {
            Id = product.Id.ToString(),
            Name = product.ProductName ?? string.Empty,
            Brand = product.Brands ?? string.Empty,
            Barcode = product.Code ?? string.Empty,
            ImageUrl = product.ImageUrl ?? string.Empty,
            Categories = product.ProductCategoryTags
                .Select(ct => ct.CategoryTag.Name)
                .ToList(),
            Ingredients = product.ProductIngredientTags
                .Select(it => it.IngredientTag.Name)
                .ToList(),
            Allergens = product.ProductAllergenTags
                .Select(at => at.AllergenTag.Name)
                .ToList(),
            Countries = product.ProductCountryTags
                .Select(ct => ct.CountryTag.Name)
                .ToList(),
            Nutrition = product.ToNutritionInfoDto(),
            NutritionGrade = product.NutritionGrade,
            EcoScoreGrade = product.EcoScoreGrade,
            Source = product.Source.ToString()
        };
    }

    public static NutritionInfoDto ToNutritionInfoDto(this Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        return new NutritionInfoDto
        {
            Calories = product.EnergyKcal100g,
            Fat = product.Fat100g,
            Carbohydrates = product.Carbohydrates100g,
            Proteins = product.Proteins100g,
            EstimatedCalories = product.estimatedCalories,
            EstimatedProteins = product.estimatedProteins,
            EstimatedCarbohydrates = product.estimatedCarbohydrates,
            EstimatedFats = product.estimatedFats
        };
    }

    public static IEnumerable<ProductDto> ToProductDtoList(this IEnumerable<Product> products)
    {
        if (products == null)
            throw new ArgumentNullException(nameof(products));

        return products.Select(p => p.ToProductDto());
    }

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

