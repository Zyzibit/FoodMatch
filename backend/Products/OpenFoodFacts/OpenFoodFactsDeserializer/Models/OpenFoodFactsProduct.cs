using System.Text.Json.Serialization;

namespace inzynierka.Products.OpenFoodFacts.OpenFoodFactsDeserializer.Models;

public class OpenFoodFactsProduct {
    [JsonPropertyName("code")]
    public string? Code { get; set; }
    [JsonPropertyName("lang")]
    public string? Language { get; set; }
    [JsonPropertyName("brand_owner")]
    public string? BrandOwner { get; set; }
    [JsonPropertyName ("lc")]
    public string? LanguageCode { get; set; }
    [JsonPropertyName("product_name")]
    public string? ProductName { get; set; }
    [JsonPropertyName("categories_tags")]
    public List<string>? CategoriesTags { get; set; }
    [JsonPropertyName("countries_tags")]
    public List<string>? CountriesTags { get; set; }
    [JsonPropertyName("countries")]
    public string? Countries { get; set; }
    [JsonPropertyName("countries_lc")]
    public string? CountriesCode { get; set; }
    [JsonPropertyName("brands")]
    public string? Brands { get; set; }
    [JsonPropertyName("categories")]
    public string? Categories { get; set; }
    [JsonPropertyName("nutrition_grades")]
    public string? NutritionGrade { get; set; }

    [JsonPropertyName("nova_group")]
    public int? NovaGroup { get; set; }

    [JsonPropertyName("ecoscore_grade")]
    public string? EcoScoreGrade { get; set; }

    [JsonPropertyName("ingredients_text")]
    public string? IngredientsText { get; set; }
    
    [JsonPropertyName("allergens_tags")]
    public List<string>? AllergensTags { get; set; }

    [JsonPropertyName("ingredients_tags")]
    public List<string>? IngredientsTags { get; set; }
    [JsonPropertyName("serving_size")]
    public string? ServingSize { get; set; }

    [JsonPropertyName("nutriments")]
    public OpenFoodFactsNutriments? OpenFoodFactsNutriments { get; set; }

    [JsonPropertyName("is_vegetarian")]
    public string? IsVegetarian { get; set; }

    [JsonPropertyName("is_vegan")]
    public string? IsVegan { get; set; }
    [JsonPropertyName("last_updated_t")]
    public long LastUpdatedT { get; set; }
    


}