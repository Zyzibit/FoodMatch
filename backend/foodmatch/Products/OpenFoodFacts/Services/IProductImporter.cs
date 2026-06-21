

namespace foodmatch.Products.OpenFoodFacts.Services
{
    public interface IProductImporter
    {
        Task ImportJsonlAsync(string filePath, CancellationToken ct = default);
    }
}
