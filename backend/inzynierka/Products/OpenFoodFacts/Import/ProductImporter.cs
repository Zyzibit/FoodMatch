using System.Text.Json;
using inzynierka.IO.Pipeline;
using inzynierka.Products.OpenFoodFacts.OpenFoodFactsDeserializer.Models;
using inzynierka.Products.Repositories;
using Microsoft.Extensions.Logging;

namespace inzynierka.Products.OpenFoodFacts.Import
{
    /// <summary>
    /// Import dumpu JSONL OpenFoodFacts. Czytanie, parsowanie, równoległość i backpressure
    /// realizuje reużywalny potok z biblioteki <c>inzynierka.IO</c>; tutaj zostaje jedynie
    /// kompozycja potoku, lifecycle bulk-importu i logowanie.
    /// </summary>
    public sealed class ProductImporter : IProductImporter
    {
        private readonly IProductBulkRepository _bulkRepository;
        private readonly ILogger<ProductImporter> _logger;

        private const int ProductBatchSize = 500;
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

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
            try
            {
                var sink = new ProductBulkSink(_bulkRepository);

                var progress = new Progress<PipelineProgress>(p =>
                    _logger.LogInformation("Imported so far: {Items} (read lines: {Read})", p.ItemsWritten, p.LinesRead));

                var report = await DataPipeline
                    .FromJsonlFile(filePath)
                    .DeserializeJson<OpenFoodFactsProduct>(JsonOpts)
                    .Configure(o =>
                    {
                        o.BatchSize = ProductBatchSize;
                        o.ErrorPolicy = ErrorPolicy.Skip;
                    })
                    .ReportProgress(progress)
                    .WriteBatchesTo(sink, ct);

                _logger.LogInformation(
                    "Import finished. Imported: {Ok}, NoNutrition: {Nutrition}, NoCode: {Code}, ParseFailed: {Parse}, TotalLines: {Lines}, Throughput: {Tput:N0} lines/s",
                    sink.Imported, sink.SkippedNoNutrition, sink.SkippedNoCode, report.Failed, report.LinesRead, report.LinesPerSecond);
            }
            finally
            {
                _bulkRepository.RestoreAfterBulkImport();
            }
        }
    }
}
