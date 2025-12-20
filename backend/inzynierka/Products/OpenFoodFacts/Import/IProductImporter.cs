

namespace inzynierka.Products.OpenFoodFacts.Import
{
    public interface IProductImporter
    {
        Task ImportJsonlAsync(string filePath, CancellationToken ct = default);
    }
}
