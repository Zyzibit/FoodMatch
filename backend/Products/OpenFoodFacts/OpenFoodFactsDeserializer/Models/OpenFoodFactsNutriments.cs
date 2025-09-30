using System.Text.Json.Serialization;

namespace inzynierka.Products.OpenFoodFacts.OpenFoodFactsDeserializer.Models;

public class OpenFoodFactsNutriments
{
    [JsonPropertyName("energy_100g")]
    public double? Energy100g { get; set; }

    [JsonPropertyName("energy-kcal_100g")]
    public double? EnergyKcal100g { get; set; }

    [JsonPropertyName("fat_100g")]
    public double? Fat100g { get; set; }

    [JsonPropertyName("saturated-fat_100g")]
    public double? SaturatedFat100g { get; set; }

    [JsonPropertyName("carbohydrates_100g")]
    public double? Carbohydrates100g { get; set; }

    [JsonPropertyName("sugars_100g")]
    public double? Sugars100g { get; set; }

    [JsonPropertyName("fiber_100g")]
    public double? Fiber100g { get; set; }

    [JsonPropertyName("proteins_100g")]
    public double? Proteins100g { get; set; }

    [JsonPropertyName("salt_100g")]
    public double? Salt100g { get; set; }

    [JsonPropertyName("sodium_100g")]
    public double? Sodium100g { get; set; }

    [JsonPropertyName("energy-kcal_serving")]
    public double? EnergyKcalServing { get; set; }
}
