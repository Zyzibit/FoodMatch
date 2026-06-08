using System.IO.Pipelines;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using FastPipe.Internal;
using FastPipe.Parsing;

namespace FastPipe.Pipeline;

/// <summary>
/// Punkt wejścia fluentowego API biblioteki.
///
/// <code>
/// var report = await DataPipeline
///     .FromJsonlFile(path)
///     .DeserializeJson&lt;OpenFoodFactsProduct&gt;()
///     .Where(p =&gt; p.Code is not null)
///     .WithParallelism(Environment.ProcessorCount)
///     .WithBatchSize(500)
///     .OnError(ErrorPolicy.Skip)
///     .WriteBatchesTo(sink, ct);
/// </code>
///
/// Źródłem może być plik, dowolny <see cref="Stream"/> (także owinięty w
/// <c>GZipStream</c>/strumień sieciowy) lub gotowy <see cref="PipeReader"/>.
/// </summary>
public static class DataPipeline
{
    /// <summary>Czytaj plik JSONL/line-delimited z systemu plików.</summary>
    public static PipelineSource FromJsonlFile(string filePath) => new(new FilePipelineSource(filePath));

    /// <summary>
    /// Czytaj z dowolnego strumienia. Dla skompresowanych danych owiń źródło, np.
    /// <c>FromStream(new GZipStream(File.OpenRead(path), CompressionMode.Decompress))</c>.
    /// </summary>
    public static PipelineSource FromStream(Stream stream, bool leaveOpen = false) =>
        new(new StreamPipelineSource(stream, leaveOpen));

    /// <summary>Czytaj z gotowego <see cref="PipeReader"/> (właścicielem pozostaje wołający).</summary>
    public static PipelineSource FromPipeReader(PipeReader reader) => new(new PipeReaderPipelineSource(reader));
}

public sealed class PipelineSource
{
    private readonly PipelineDataSource _source;
    private readonly PipelineOptions _options = new();

    internal PipelineSource(PipelineDataSource source) => _source = source;

    public PipelineSource Configure(Action<PipelineOptions> configure)
    {
        configure(_options);
        return this;
    }

    /// <summary>Parsuj każdą linię jako JSON do <typeparamref name="T"/> (z bajtów UTF-8, reflection-based).</summary>
    public DataPipelineBuilder<T> DeserializeJson<T>(JsonSerializerOptions? jsonOptions = null) =>
        Parse(new JsonRecordParser<T>(jsonOptions));

    /// <summary>Parsuj jako JSON używając metadanych source-gen (<c>JsonSerializerContext</c>) — bez refleksji.</summary>
    public DataPipelineBuilder<T> DeserializeJson<T>(JsonTypeInfo<T> typeInfo) =>
        Parse(new JsonRecordParser<T>(typeInfo));

    /// <summary>Użyj własnego, ręcznie pisanego parsera (np. <c>Utf8JsonReader</c> po polach).</summary>
    public DataPipelineBuilder<T> Parse<T>(IRecordParser<T> parser)
    {
        RecordProjection<T> projection = (ReadOnlyMemory<byte> line, out T value) => parser.TryParse(line.Span, out value);
        return new DataPipelineBuilder<T>(_source, _options, projection, progress: null);
    }
}

/// <summary>Konfigurowalny, niemutowalny w łańcuchu builder potoku dla rekordu typu <typeparamref name="T"/>.</summary>
public sealed class DataPipelineBuilder<T>
{
    private readonly PipelineDataSource _source;
    private readonly PipelineOptions _options;
    private readonly RecordProjection<T> _projection;
    private IProgress<PipelineProgress>? _progress;

    internal DataPipelineBuilder(
        PipelineDataSource source, PipelineOptions options, RecordProjection<T> projection, IProgress<PipelineProgress>? progress)
    {
        _source = source;
        _options = options;
        _projection = projection;
        _progress = progress;
    }

    public DataPipelineBuilder<T> Configure(Action<PipelineOptions> configure) { configure(_options); return this; }
    public DataPipelineBuilder<T> WithParallelism(int workers) { _options.Parallelism = workers; return this; }
    public DataPipelineBuilder<T> WithBatchSize(int size) { _options.BatchSize = size; return this; }
    public DataPipelineBuilder<T> WithChannelCapacity(int capacity) { _options.ChannelCapacity = capacity; return this; }
    public DataPipelineBuilder<T> OnError(ErrorPolicy policy) { _options.ErrorPolicy = policy; return this; }
    public DataPipelineBuilder<T> ReportProgress(IProgress<PipelineProgress> progress) { _progress = progress; return this; }

    /// <summary>Odrzuć rekordy nie spełniające predykatu (liczone jako Dropped).</summary>
    public DataPipelineBuilder<T> Where(Func<T, bool> predicate)
    {
        var prev = _projection;
        RecordProjection<T> next = (ReadOnlyMemory<byte> line, out T value) => prev(line, out value) && predicate(value);
        return new DataPipelineBuilder<T>(_source, _options, next, _progress);
    }

    /// <summary>Zmapuj rekord na inny typ.</summary>
    public DataPipelineBuilder<TOut> Select<TOut>(Func<T, TOut> map)
    {
        var prev = _projection;
        RecordProjection<TOut> next = (ReadOnlyMemory<byte> line, out TOut value) =>
        {
            if (prev(line, out var parsed))
            {
                value = map(parsed);
                return true;
            }
            value = default!;
            return false;
        };
        return new DataPipelineBuilder<TOut>(_source, _options, next, _progress);
    }

    // ---- Terminale ----------------------------------------------------------

    /// <summary>Tryb PUSH: batchuj i zapisuj do sinka. Zwraca raport przebiegu.</summary>
    public Task<IngestionReport> WriteBatchesTo(IBatchSink<T> sink, CancellationToken cancellationToken = default) =>
        PipelineExecutor.RunAsync(_source, _projection, sink, _options, _progress, cancellationToken);

    /// <summary>Tryb PUSH z sinkiem-delegatem.</summary>
    public Task<IngestionReport> WriteBatchesTo(
        Func<IReadOnlyList<T>, CancellationToken, ValueTask> writeBatch, CancellationToken cancellationToken = default) =>
        WriteBatchesTo(new DelegateBatchSink<T>(writeBatch), cancellationToken);

    /// <summary>Tryb PULL: strumień rekordów jako <see cref="IAsyncEnumerable{T}"/>.</summary>
    public IAsyncEnumerable<T> StreamAsync(CancellationToken cancellationToken = default) =>
        PipelineExecutor.StreamAsync(_source, _projection, _options, cancellationToken);
}
