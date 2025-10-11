using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using inzynierka.Products.OpenFoodFacts.OpenFoodFactsDeserializer.Models;

namespace inzynierka.Products.OpenFoodFacts.OpenFoodFactsDeserializer.Services;

public class OpenFoodFactsDeserializer : IOpenFoodFactsDeserializer
{
    private readonly ILogger<OpenFoodFactsDeserializer> _logger;

    // ⚙️ Stałe opcje JSON – zainicjalizowane raz, reużywane (bez GC)
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    // ⚙️ Ilość równoległych workerów do deserializacji
    private const int WORKER_COUNT = 8;

    public OpenFoodFactsDeserializer(ILogger<OpenFoodFactsDeserializer> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Szybki, równoległy deserializer dużych plików JSONL.
    /// </summary>
    public async IAsyncEnumerable<OpenFoodFactsProduct> DeserializeFromJsonlFileAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        var channel = Channel.CreateBounded<string>(new BoundedChannelOptions(10_000)
        {
            SingleWriter = true,
            FullMode = BoundedChannelFullMode.Wait
        });

        var producer = Task.Run(async () =>
        {
            try
            {
                using var fs = new FileStream(
                    filePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    bufferSize: 1_048_576, 
                    useAsync: true);

                using var reader = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1_048_576);

                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    if (line.IndexOf('\0') >= 0)
                        line = line.Replace("\0", string.Empty);

                    await channel.Writer.WriteAsync(line, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "File reading failed: {File}", filePath);
            }
            finally
            {
                channel.Writer.Complete();
            }
        }, cancellationToken);

        var output = Channel.CreateUnbounded<OpenFoodFactsProduct>();

        var consumers = Enumerable.Range(0, WORKER_COUNT)
            .Select(_ => Task.Run(async () =>
            {
                await foreach (var line in channel.Reader.ReadAllAsync(cancellationToken))
                {
                    try
                    {
                        var product = JsonSerializer.Deserialize<OpenFoodFactsProduct>(line, _jsonOptions);
                        if (product != null)
                            await output.Writer.WriteAsync(product, cancellationToken);
                    }
                    catch (JsonException)
                    {
                        _logger.LogWarning("Invalid JSON line skipped");
                    }
                }
            }, cancellationToken))
            .ToArray();

        _ = Task.Run(async () =>
        {
            await Task.WhenAll(consumers);
            output.Writer.Complete();
        }, cancellationToken);

        await foreach (var product in output.Reader.ReadAllAsync(cancellationToken))
        {
            yield return product;
        }

        await producer;
    }
}
