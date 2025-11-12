using inzynierka.Products.Dto;
using inzynierka.Products.Responses;
using inzynierka.Products.Model;

namespace inzynierka.Products.Mappings;

public class ProductMapper : IProductMapper
{
    public ProductDto MapToProductInfo(Product product)
    {
        return new ProductDto
        {
            Id = product.Id.ToString(),
            Name = product.ProductName ?? "",
            Brand = product.Brands ?? "",
            Barcode = product.Code,
            ImageUrl = product.ImageUrl ?? "",
            Categories = product.ProductCategoryTags.Select(pct => pct.CategoryTag.Name).ToList(),
            Ingredients = product.ProductIngredientTags.Select(pit => pit.IngredientTag.Name).ToList(),
            Allergens = product.ProductAllergenTags.Select(pat => pat.AllergenTag.Name).ToList(),
            Countries = product.ProductCountryTags.Select(pct => pct.CountryTag.Name).ToList(),
            NutritionGrade = product.NutritionGrade,
            Nutrition = MapToNutritionInfo(product),
            EcoScoreGrade = product.EcoScoreGrade,
            IsAiGenerated = product.IsAiGenerated,

        };
    }

    public NutritionInfoDto MapToNutritionInfo(Product product)
    {
        return new NutritionInfoDto
        {
            Carbohydrates = product.Carbohydrates100g,
            Proteins = product.Proteins100g,
            Fat = product.Fat100g,
            Calories = product.EnergyKcal100g,
            EstimatedCalories = product.estimatedCalories,
            EstimatedProteins = product.estimatedProteins,
            EstimatedCarbohydrates = product.estimatedCarbohydrates,
            EstimatedFats = product.estimatedFats,
        };
    }

    public IEnumerable<ProductDto> MapToProductInfoList(IEnumerable<Product> products)
    {
        return products.Select(MapToProductInfo);
    }
}
