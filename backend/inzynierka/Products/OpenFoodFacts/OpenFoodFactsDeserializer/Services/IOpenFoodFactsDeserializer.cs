using inzynierka.Products.OpenFoodFacts.OpenFoodFactsDeserializer.Models;
using System.Runtime.CompilerServices;

namespace inzynierka.Products.OpenFoodFacts.OpenFoodFactsDeserializer.Services;

public interface IOpenFoodFactsDeserializer
{
    IAsyncEnumerable<OpenFoodFactsProduct> DeserializeFromJsonlFileAsync(
    string filePath,
    CancellationToken cancellationToken = default);
}