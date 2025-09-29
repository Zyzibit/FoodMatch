using inzynierka.OpenFoodFacts.JsonlReader.Models;
using inzynierka.OpenFoodFacts.Models;

namespace inzynierka.OpenFoodFacts.JsonlReader.Services;

public interface IOpenFoodFactsDeserializer
{
    IAsyncEnumerable<OpenFoodFactsProduct> DeserializeFromJsonlFileAsync(string filePath) ;
}