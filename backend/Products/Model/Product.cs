using System.ComponentModel.DataAnnotations;
using inzynierka.Products.Model.Tag.AllergenTag;
using inzynierka.Products.Model.Tag.CategoryTag;
using inzynierka.Products.Model.Tag.CountryTag;
using inzynierka.Products.Model.Tag.IngredientTag;
using inzynierka.Receipts.Model;

namespace inzynierka.Products.Model;

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

    public ICollection<ProductIngredientTag> ProductIngredientTags { get; set; } = new List<ProductIngredientTag>();
    public ICollection<ProductCountryTag> ProductCountryTags { get; set; } = new List<ProductCountryTag>();
    public ICollection<ProductCategoryTag> ProductCategoryTags { get; set; } = new List<ProductCategoryTag>();
    public ICollection<ProductAllergenTag> ProductAllergenTags { get; set; } = new List<ProductAllergenTag>();
    
    public ICollection<ReceiptIngredient> ReceiptIngredients { get; set; } = new List<ReceiptIngredient>();
    
    public string? Brands { get; set; }
    public string? NutritionGrade { get; set; }
    public int? NovaGroup { get; set; }
    public string? EcoScoreGrade { get; set; }
    public string? IngredientsText { get; set; }
    public string? ServingSize { get; set; }
    public string? IsVegetarian { get; set; }
    public string? IsVegan { get; set; }
    public string? ImageUrl { get; set; }
    
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

