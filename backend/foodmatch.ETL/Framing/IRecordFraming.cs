using System.Buffers;

namespace foodmatch.ETL.Framing;

/// <summary>
/// Splits a byte stream into record slices. This is the seam that makes the engine
/// format-agnostic: line-delimited (default), a custom delimiter, CSV (quoted multiline), etc.
/// The engine handles buffering/backpressure and calls these methods on the producer thread.
/// </summary>
public interface IRecordFraming
{
    /// <summary>
    /// Tries to cut one complete record from the front of <paramref name="buffer"/>. On success
    /// advances <paramref name="buffer"/> past the record (and its delimiter) and returns the
    /// record slice (delimiter excluded). Returns <c>false</c> when more bytes are needed.
    /// </summary>
    bool TryReadRecord(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> record);

    /// <summary>
    /// Emits the trailing bytes once the source is complete — i.e. a final record that has no
    /// terminating delimiter. Returns <c>false</c> when there is nothing left to emit.
    /// </summary>
    bool TryReadTrailing(in ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> record)
    {
        record = buffer;
        return !buffer.IsEmpty;
    }

    /// <summary>
    /// True when the record is empty/whitespace and should be skipped (when enabled).
    /// Default: only ASCII whitespace and NUL bytes. Override for format-specific emptiness.
    /// </summary>
    bool IsBlank(in ReadOnlySequence<byte> record)
    {
        foreach (var segment in record)
        {
            foreach (var b in segment.Span)
            {
                if (b is not ((byte)' ' or (byte)'\t' or (byte)'\r' or (byte)'\n' or 0))
                    return false;
            }
        }
        return true;
    }
}
