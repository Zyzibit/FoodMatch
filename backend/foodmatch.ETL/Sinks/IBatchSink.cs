namespace foodmatch.ETL.Sinks;

/// <summary>
/// Receiver of record batches — the pipeline's destination (e.g. a bulk write to a database
/// via PostgreSQL binary COPY). The engine guarantees that <see cref="WriteBatchAsync"/> is
/// invoked sequentially (one batch at a time), so the implementation need not be thread-safe.
/// </summary>
public interface IBatchSink<T>
{
    ValueTask WriteBatchAsync(IReadOnlyList<T> batch, CancellationToken cancellationToken);
}

/// <summary>
/// Delegate-backed sink — convenient for simple cases and tests.
/// </summary>
public sealed class DelegateBatchSink<T> : IBatchSink<T>
{
    private readonly Func<IReadOnlyList<T>, CancellationToken, ValueTask> _write;

    public DelegateBatchSink(Func<IReadOnlyList<T>, CancellationToken, ValueTask> write) => _write = write;

    public ValueTask WriteBatchAsync(IReadOnlyList<T> batch, CancellationToken cancellationToken) =>
        _write(batch, cancellationToken);
}
