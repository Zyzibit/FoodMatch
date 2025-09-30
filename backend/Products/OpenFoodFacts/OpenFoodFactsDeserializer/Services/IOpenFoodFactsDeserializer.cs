using inzynierka.Products.OpenFoodFacts.OpenFoodFactsDeserializer.Models;

namespace inzynierka.Products.OpenFoodFacts.OpenFoodFactsDeserializer.Services;

public interface IOpenFoodFactsDeserializer
{
    IAsyncEnumerable<OpenFoodFactsProduct> DeserializeFromJsonlFileAsync(string filePath) ;
}