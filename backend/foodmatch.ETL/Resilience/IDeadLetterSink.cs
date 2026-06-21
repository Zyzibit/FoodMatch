namespace foodmatch.ETL.Resilience;

/// <summary>
/// Destination for records that failed to parse. With <see cref="ErrorPolicy.DeadLetter"/> the
/// engine routes the raw bytes of each malformed record here (with the exception) and continues,
/// instead of only counting the failure. Implementations are called sequentially from a worker;
/// the bytes are a short-lived copy valid only for the duration of the call.
/// </summary>
public interface IDeadLetterSink
{
    ValueTask WriteAsync(ReadOnlyMemory<byte> rawRecord, Exception error, CancellationToken cancellationToken);
}
