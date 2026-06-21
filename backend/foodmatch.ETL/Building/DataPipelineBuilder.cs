using System.Runtime.CompilerServices;
using foodmatch.ETL.Diagnostics;
using foodmatch.ETL.Engine;
using foodmatch.ETL.Resilience;
using foodmatch.ETL.Sinks;
using foodmatch.ETL.Sources;

namespace foodmatch.ETL.Building;

/// <summary>A pipeline builder for records of type <typeparamref name="T"/> (immutable along the chain).</summary>
public sealed class DataPipelineBuilder<T>
{
    private readonly IReadOnlyList<SourceEntry> _sources;
    private readonly PipelineOptions _options;
    private readonly RecordProjection<T> _projection;
    private IProgress<PipelineProgress>? _progress;
    private ICheckpointStore? _checkpoint;
    private string? _checkpointPath;

    internal DataPipelineBuilder(
        IReadOnlyList<SourceEntry> sources, PipelineOptions options, RecordProjection<T> projection,
        IProgress<PipelineProgress>? progress, ICheckpointStore? checkpoint, string? checkpointPath)
    {
        _sources = sources;
        _options = options;
        _projection = projection;
        _progress = progress;
        _checkpoint = checkpoint;
        _checkpointPath = checkpointPath;
    }

    public DataPipelineBuilder<T> Configure(Action<PipelineOptions> configure) { configure(_options); return this; }
    public DataPipelineBuilder<T> WithParallelism(int workers) { _options.Parallelism = workers; return this; }
    public DataPipelineBuilder<T> WithBatchSize(int size) { _options.BatchSize = size; return this; }
    public DataPipelineBuilder<T> WithChannelCapacity(int capacity) { _options.ChannelCapacity = capacity; return this; }
    public DataPipelineBuilder<T> OnError(ErrorPolicy policy) { _options.ErrorPolicy = policy; return this; }
    public DataPipelineBuilder<T> ReportProgress(IProgress<PipelineProgress> progress) { _progress = progress; return this; }
    public DataPipelineBuilder<T> WithCheckpoint(string checkpointPath) { _checkpointPath = checkpointPath; return this; }
    public DataPipelineBuilder<T> WithCheckpoint(ICheckpointStore store) { _checkpoint = store; return this; }

    /// <summary>Route malformed records to a dead-letter sink and continue (counts them as Failed).</summary>
    public DataPipelineBuilder<T> OnErrorDeadLetter(IDeadLetterSink sink)
    {
        _options.ErrorPolicy = ErrorPolicy.DeadLetter;
        _options.DeadLetterSink = sink;
        return this;
    }

    /// <summary>Drop records that fail the predicate (counted as Dropped).</summary>
    public DataPipelineBuilder<T> Where(Func<T, bool> predicate)
    {
        var prev = _projection;
        RecordProjection<T> next = line =>
        {
            var vt = prev(line);
            if (vt.IsCompletedSuccessfully)
            {
                var o = vt.Result;
                return new ValueTask<ProjectionOutcome<T>>(
                    o.Produced && predicate(o.Value) ? o : ProjectionOutcome<T>.Drop);
            }
            return AwaitWhere(vt, predicate);
        };
        return With(next);
    }

    /// <summary>Drop records that fail an async predicate (counted as Dropped).</summary>
    public DataPipelineBuilder<T> WhereAsync(Func<T, ValueTask<bool>> predicate)
    {
        var prev = _projection;
        RecordProjection<T> next = async line =>
        {
            var o = await prev(line);
            if (!o.Produced) return o;
            return await predicate(o.Value) ? o : ProjectionOutcome<T>.Drop;
        };
        return With(next);
    }

    /// <summary>Map a record to another type.</summary>
    public DataPipelineBuilder<TOut> Select<TOut>(Func<T, TOut> map)
    {
        var prev = _projection;
        RecordProjection<TOut> next = line =>
        {
            var vt = prev(line);
            if (vt.IsCompletedSuccessfully)
            {
                var o = vt.Result;
                return new ValueTask<ProjectionOutcome<TOut>>(
                    o.Produced ? ProjectionOutcome<TOut>.Keep(map(o.Value)) : ProjectionOutcome<TOut>.Drop);
            }
            return AwaitSelect(vt, map);
        };
        return new DataPipelineBuilder<TOut>(_sources, _options, next, _progress, _checkpoint, _checkpointPath);
    }

    /// <summary>Map a record to another type with an async mapper.</summary>
    public DataPipelineBuilder<TOut> SelectAsync<TOut>(Func<T, ValueTask<TOut>> map)
    {
        var prev = _projection;
        RecordProjection<TOut> next = async line =>
        {
            var o = await prev(line);
            return o.Produced ? ProjectionOutcome<TOut>.Keep(await map(o.Value)) : ProjectionOutcome<TOut>.Drop;
        };
        return new DataPipelineBuilder<TOut>(_sources, _options, next, _progress, _checkpoint, _checkpointPath);
    }

    private DataPipelineBuilder<T> With(RecordProjection<T> projection) =>
        new(_sources, _options, projection, _progress, _checkpoint, _checkpointPath);

    private static async ValueTask<ProjectionOutcome<T>> AwaitWhere(ValueTask<ProjectionOutcome<T>> vt, Func<T, bool> predicate)
    {
        var o = await vt;
        return o.Produced && predicate(o.Value) ? o : ProjectionOutcome<T>.Drop;
    }

    private static async ValueTask<ProjectionOutcome<TOut>> AwaitSelect<TOut>(ValueTask<ProjectionOutcome<T>> vt, Func<T, TOut> map)
    {
        var o = await vt;
        return o.Produced ? ProjectionOutcome<TOut>.Keep(map(o.Value)) : ProjectionOutcome<TOut>.Drop;
    }

    // ---- Terminals ----------------------------------------------------------

    /// <summary>PUSH mode: batch and write to the sink. Returns an aggregated run report across all sources.</summary>
    public async Task<IngestionReport> WriteBatchesTo(IBatchSink<T> sink, CancellationToken cancellationToken = default)
    {
        var checkpoint = await ResolveCheckpointAsync(cancellationToken);
        IngestionReport? total = null;

        foreach (var entry in _sources)
        {
            if (checkpoint is not null && entry.Key is not null && checkpoint.IsDone(entry.Key))
                continue;

            var report = await PipelineExecutor.RunAsync(entry.Source, _projection, sink, _options, _progress, cancellationToken);
            total = total is null ? report : total + report;

            if (checkpoint is not null && entry.Key is not null)
                await checkpoint.MarkDoneAsync(entry.Key, cancellationToken);
        }

        return total ?? new IngestionReport();
    }

    /// <summary>PUSH mode with a delegate sink.</summary>
    public Task<IngestionReport> WriteBatchesTo(
        Func<IReadOnlyList<T>, CancellationToken, ValueTask> writeBatch, CancellationToken cancellationToken = default) =>
        WriteBatchesTo(new DelegateBatchSink<T>(writeBatch), cancellationToken);

    /// <summary>PULL mode: stream records as an <see cref="IAsyncEnumerable{T}"/> across all sources.</summary>
    public async IAsyncEnumerable<T> StreamAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var checkpoint = await ResolveCheckpointAsync(cancellationToken);

        foreach (var entry in _sources)
        {
            if (checkpoint is not null && entry.Key is not null && checkpoint.IsDone(entry.Key))
                continue;

            await foreach (var item in PipelineExecutor.StreamAsync(entry.Source, _projection, _options, cancellationToken))
                yield return item;

            // Reached only when the source streamed fully (an early break skips marking it done).
            if (checkpoint is not null && entry.Key is not null)
                await checkpoint.MarkDoneAsync(entry.Key, cancellationToken);
        }
    }

    private async Task<ICheckpointStore?> ResolveCheckpointAsync(CancellationToken ct) =>
        _checkpoint ?? (_checkpointPath is null ? null : await FileCheckpointStore.LoadAsync(_checkpointPath, ct));


}
