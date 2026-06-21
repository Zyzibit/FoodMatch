using System.Text.Json;
using System.Text.Json.Serialization;
using foodmatch.Products.OpenFoodFacts.Models;

namespace foodmatch.Products.OpenFoodFacts.Services
{
    /// <summary>
    /// Kontekst source-gen System.Text.Json dla rekordów OpenFoodFacts.
    /// Metadane parsowania są generowane w czasie kompilacji (zero refleksji w runtime),
    /// co przy 60 GB i milionach linii daje realny zysk względem reflection-based deserializacji.
    ///
    /// Case-insensitive wyłączone celowo — model ma dokładne [JsonPropertyName] zgodne z kluczami OFF.
    /// </summary>
    [JsonSourceGenerationOptions(
        PropertyNameCaseInsensitive = false,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip)]
    [JsonSerializable(typeof(OpenFoodFactsProduct))]
    internal partial class OpenFoodFactsJsonContext : JsonSerializerContext
    {
    }
}
