using foodmatch.Products.OpenFoodFacts.Services;
using foodmatch.Products.Services;

namespace foodmatch.Products.OpenFoodFacts.Adapters;

public class OpenFoodFactsImportServiceAdapter : IProductImportService
{
    private readonly IProductImporter _openFoodFactsImporter;

    public OpenFoodFactsImportServiceAdapter(IProductImporter openFoodFactsImporter)
    {
        _openFoodFactsImporter = openFoodFactsImporter;
    }

    public async Task ImportProductsAsync(string filePath, CancellationToken ct = default)
    {
        await _openFoodFactsImporter.ImportJsonlAsync(filePath, ct);
    }
}

