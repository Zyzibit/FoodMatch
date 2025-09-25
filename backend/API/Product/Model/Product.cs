using System.ComponentModel.DataAnnotations;
using inzynierka.API.Product.Model.Tag.AllergenTag;
using inzynierka.API.Product.Model.Tag.CategoryTag;
using inzynierka.API.Product.Model.Tag.CountryTag;
using inzynierka.API.Product.Model.Tag.IngredientTag;
using inzynierka.API.Shared.Model;

namespace inzynierka.API.Product.Model;

public class Product 
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Code { get; set; }
    public string? Language { get; set; }
    public string? BrandOwner { get; set; }
    public string? LanguageCode { get; set; }
    public string? ProductName { get; set; }
    public ICollection<UserProduct> UserProducts { get; set; }

    
    public ICollection<ProductIngredientTag> ProductIngredientTags { get; set; }
    public ICollection<ProductCountryTag> ProductCountryTags { get; set; }
    public ICollection<ProductCategoryTag> ProductCategoryTags { get; set; }
    public ICollection<ProductAllergenTag> ProductAllergenTags { get; set; }
    
    public string? Countries { get; set; }
    public string? CountriesCode { get; set; }
    public string? Brands { get; set; }
    public string? Categories { get; set; }
    public string? NutritionGrade { get; set; }
    public int? NovaGroup { get; set; }
    public string? EcoScoreGrade { get; set; }
    public string? IngredientsText { get; set; }
    public string? ServingSize { get; set; }
    public string? IsVegetarian { get; set; }
    public string? IsVegan { get; set; }
    public double? Energy100g { get; set; }
    public double? EnergyKcal100g { get; set; }
    public double? Fat100g { get; set; }
    public double? SaturatedFat100g { get; set; }
    public double? Carbohydrates100g { get; set; }
    public double? Sugars100g { get; set; }
    public double? Fiber100g { get; set; }
    public double? Proteins100g { get; set; }
    public double? Salt100g { get; set; }
    public double? Sodium100g { get; set; }
    public double? EnergyKcalServing { get; set; }
    public DateTime? LastUpdated { get; set; }
}

