namespace FastPipe.Pipeline;

/// <summary>
/// Odbiornik paczek rekordów — miejsce docelowe potoku (np. masowy zapis do bazy
/// przez PostgreSQL binary COPY). Silnik gwarantuje, że <see cref="WriteBatchAsync"/>
/// jest wywoływane sekwencyjnie (jedna paczka naraz), więc implementacja nie musi
/// być thread-safe.
/// </summary>
public interface IBatchSink<T>
{
    ValueTask WriteBatchAsync(IReadOnlyList<T> batch, CancellationToken cancellationToken);
}

/// <summary>
/// Sink z delegatu — wygodny do prostych przypadków i testów.
/// </summary>
public sealed class DelegateBatchSink<T> : IBatchSink<T>
{
    private readonly Func<IReadOnlyList<T>, CancellationToken, ValueTask> _write;

    public DelegateBatchSink(Func<IReadOnlyList<T>, CancellationToken, ValueTask> write) => _write = write;

    public ValueTask WriteBatchAsync(IReadOnlyList<T> batch, CancellationToken cancellationToken) =>
        _write(batch, cancellationToken);
}
