using System.Text.Json;
using System.Text.Json.Serialization;
using inzynierka.Products.OpenFoodFacts.OpenFoodFactsDeserializer.Models;

namespace inzynierka.Products.OpenFoodFacts.Import
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
