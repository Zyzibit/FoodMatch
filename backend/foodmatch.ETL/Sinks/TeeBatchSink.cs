namespace foodmatch.ETL.Sinks;

/// <summary>
/// Decorator that fans the same batch out to several sinks, written sequentially in order.
/// If a sink throws, the remaining sinks for that batch are not invoked and the exception
/// propagates (combine with <see cref="RetryBatchSink{T}"/> per sink if needed).
/// </summary>
public sealed class TeeBatchSink<T> : IBatchSink<T>
{
    private readonly IBatchSink<T>[] _sinks;

    public TeeBatchSink(params IBatchSink<T>[] sinks)
    {
        ArgumentNullException.ThrowIfNull(sinks);
        ArgumentOutOfRangeException.ThrowIfZero(sinks.Length, nameof(sinks));
        _sinks = sinks;
    }

    public async ValueTask WriteBatchAsync(IReadOnlyList<T> batch, CancellationToken cancellationToken)
    {
        foreach (var sink in _sinks)
            await sink.WriteBatchAsync(batch, cancellationToken);
    }
}
