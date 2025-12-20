using inzynierka.Products.OpenFoodFacts.Import;

namespace inzynierka.Products.OpenFoodFacts.Adapters;

public class OpenFoodFactsImportServiceAdapter : Services.IProductImportService
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

