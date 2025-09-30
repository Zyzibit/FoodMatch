using System.Text.Json;
using inzynierka.Products.OpenFoodFacts.OpenFoodFactsDeserializer.Models;

namespace inzynierka.Products.OpenFoodFacts.OpenFoodFactsDeserializer.Services;




public class OpenFoodFactsDeserializer : IOpenFoodFactsDeserializer
{
    private readonly ILogger<OpenFoodFactsDeserializer> _logger;

    public OpenFoodFactsDeserializer(ILogger<OpenFoodFactsDeserializer> logger)
    {
        _logger = logger;
    }

    public async IAsyncEnumerable<OpenFoodFactsProduct> DeserializeFromJsonlFileAsync(string filePath) {
        var options = new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        using var fileReader = new StreamReader(filePath);
        int lineNumber = 0;
        string? line;

        while ((line = await fileReader.ReadLineAsync().ConfigureAwait(false)) != null){
            if (string.IsNullOrWhiteSpace(line))
                continue;
            OpenFoodFactsProduct? product = TryDeserializeProduct(line, lineNumber, options);
            if (product != null) {
                yield return product;
            }
        }
    }
    private OpenFoodFactsProduct? TryDeserializeProduct(string jsonLine, int lineNumber, JsonSerializerOptions options)
    {
        try {
            return JsonSerializer.Deserialize<OpenFoodFactsProduct>(jsonLine, options);
        }
        catch (JsonException ex) {
            _logger.LogWarning(ex, "Deserialization problem at line {Line}", lineNumber);
            return null;
        }
    }
}