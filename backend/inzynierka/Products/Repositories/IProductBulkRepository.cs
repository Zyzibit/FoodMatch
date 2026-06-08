namespace inzynierka.Products.Repositories;

public interface IProductBulkRepository
{
    void PrepareForBulkImport();
    void RestoreAfterBulkImport();

    /// <summary>
    /// Importuje całą paczkę (produkty + linki tagów) w jednej transakcji:
    /// binary COPY do tabel tymczasowych, upsert produktów (ON CONFLICT),
    /// dosianie brakujących tagów i MERGE linków.
    /// </summary>
    Task BulkImportBatchAsync(ProductBatch batch, CancellationToken ct = default);
}
