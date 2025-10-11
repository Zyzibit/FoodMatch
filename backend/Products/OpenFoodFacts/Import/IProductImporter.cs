namespace inzynierka.Products.OpenFoodFacts.Import
{
    public interface IProductImporter
    {
        Task<ImportResult> ImportAsync(string path, int maxProducts, int batchSize);
    }
}