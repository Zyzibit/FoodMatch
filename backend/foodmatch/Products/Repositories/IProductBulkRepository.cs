namespace foodmatch.Products.Repositories;

public interface IProductBulkRepository
{
    void PrepareForBulkImport();
    void RestoreAfterBulkImport();

    /// <summary>
    /// Imports a whole batch (products + tag links) in a single transaction:
    /// binary COPY into temp tables, product upsert (ON CONFLICT),
    /// seeding of missing tags and a MERGE of links.
    /// </summary>
    /// <returns>Number of product rows actually written (inserted + updated).</returns>
    Task<int> BulkImportBatchAsync(ProductBatch batch, CancellationToken ct = default);
}
