using System.Buffers;

namespace foodmatch.ETL.Framing;

/// <summary>
/// Default framing: one record per line, split on '\n' (the delimiter is excluded; a trailing
/// '\r' from CRLF stays in the record and is tolerated as JSON whitespace). The final line is
/// emitted even without a terminating newline.
/// </summary>
public sealed class LineFraming : IRecordFraming
{
    public static LineFraming Instance { get; } = new();

    public bool TryReadRecord(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> record)
    {
        var seq = new SequenceReader<byte>(buffer);
        if (seq.TryReadTo(out record, (byte)'\n', advancePastDelimiter: true))
        {
            buffer = buffer.Slice(seq.Position);
            return true;
        }

        record = default;
        return false;
    }


}
