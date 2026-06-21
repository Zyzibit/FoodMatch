namespace inzynierka.ETL.Sinks;

/// <summary>
/// Decorator that retries <see cref="IBatchSink{T}.WriteBatchAsync"/> on failure with a fixed
/// backoff. After <c>maxRetries</c> exhausted attempts the last exception is rethrown.
/// Cancellation is never retried.
/// </summary>
public sealed class RetryBatchSink<T> : IBatchSink<T>
{
    private readonly IBatchSink<T> _inner;
    private readonly int _maxRetries;
    private readonly TimeSpan _backoff;

    public RetryBatchSink(IBatchSink<T> inner, int maxRetries = 3, TimeSpan? backoff = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxRetries);
        _inner = inner;
        _maxRetries = maxRetries;
        _backoff = backoff ?? TimeSpan.FromMilliseconds(200);
    }

    public async ValueTask WriteBatchAsync(IReadOnlyList<T> batch, CancellationToken cancellationToken)
    {
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                await _inner.WriteBatchAsync(batch, cancellationToken);
                return;
            }
            catch (Exception ex) when (ex is not OperationCanceledException && attempt < _maxRetries)
            {
                await Task.Delay(_backoff, cancellationToken);
            }
        }
    }
}
