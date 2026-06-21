using System.Text.Json;
using System.Text.Json.Serialization;
using foodmatch.Products.OpenFoodFacts.Models;

namespace foodmatch.Benchmarks;

[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = false,
    AllowTrailingCommas = true,
    ReadCommentHandling = JsonCommentHandling.Skip)]
[JsonSerializable(typeof(OpenFoodFactsProduct))]
internal partial class BenchmarkJsonContext : JsonSerializerContext
{
}
