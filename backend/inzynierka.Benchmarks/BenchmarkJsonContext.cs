using System.Text.Json;
using System.Text.Json.Serialization;
using inzynierka.Products.OpenFoodFacts.OpenFoodFactsDeserializer.Models;

namespace inzynierka.Benchmarks;

[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = false,
    AllowTrailingCommas = true,
    ReadCommentHandling = JsonCommentHandling.Skip)]
[JsonSerializable(typeof(OpenFoodFactsProduct))]
internal partial class BenchmarkJsonContext : JsonSerializerContext
{
}
