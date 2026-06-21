namespace foodmatch.Products.Services;

public interface IProductImportService
{
    Task ImportProductsAsync(string filePath, CancellationToken ct = default);
}

