using inzynierka.ETL;
using inzynierka.ETL.Diagnostics;
using inzynierka.ETL.Resilience;
using inzynierka.Products.Repositories;
using Microsoft.Extensions.Logging;

namespace inzynierka.Products.OpenFoodFacts.Import
{
    /// <summary>
    /// Imports an OpenFoodFacts JSONL dump. Reading, parsing, parallelism and backpressure are
    /// handled by the reusable pipeline from the <c>inzynierka.ETL</c> library; what remains here is
    /// just pipeline composition, the bulk-import lifecycle and logging.
    /// </summary>
    public sealed class ProductImporter : IProductImporter
    {
        private readonly IProductBulkRepository _bulkRepository;
        private readonly ILogger<ProductImporter> _logger;

        // A large batch amortizes the per-batch fixed cost (CREATE TEMP + COPY + upsert/MERGE + COMMIT).
        // The whole batch runs in one transaction, so this is the main throughput lever at 60 GB.
        private const int ProductBatchSize = 20_000;

        public ProductImporter(IProductBulkRepository bulkRepository, ILogger<ProductImporter> logger)
        {
            _bulkRepository = bulkRepository;
            _logger = logger;
        }

        public async Task ImportJsonlAsync(string filePath, CancellationToken ct = default)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Import file not found: {filePath}", filePath);

            _bulkRepository.PrepareForBulkImport();
            var sink = new ProductBulkSink(_bulkRepository);
            try
            {
                var progress = new Progress<PipelineProgress>(p =>
                    _logger.LogInformation("Pipeline status: sent to sink {Items} (read lines: {Read}). Real DB inserted so far: {DbImported}", 
                        p.ItemsWritten, p.LinesRead, sink.Imported));

                var report = await DataPipeline
                    .FromJsonlFile(filePath)
                    .DeserializeJson(OpenFoodFactsJsonContext.Default.OpenFoodFactsProduct)
                    .Configure(o =>
                    {
                        o.BatchSize = ProductBatchSize;
                        o.ErrorPolicy = ErrorPolicy.Skip;
                    })
                    .Where(p => p.OpenFoodFactsNutriments != null)
                    .ReportProgress(progress)
                    .WriteBatchesTo(sink, ct);

                _logger.LogInformation(
                    "Import finished. Imported: {Ok}, NoNutrition: {Nutrition}, NoCode: {Code}, ParseFailed: {Parse}, TotalLines: {Lines}, Throughput: {Tput:N0} lines/s",
                    sink.Imported, sink.SkippedNoNutrition, sink.SkippedNoCode, report.Failed, report.LinesRead, report.LinesPerSecond);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning(
                    "Import cancelled by user (or client). DB imported until cancellation: {Ok}, Skipped NoNutrition: {Nutrition}, Skipped NoCode: {Code}",
                    sink.Imported, sink.SkippedNoNutrition, sink.SkippedNoCode);
                throw;
            }
            finally
            {
                _bulkRepository.RestoreAfterBulkImport();
            }
        }
    }
}
