using System.Buffers;
using System.Text;

namespace foodmatch.ETL.Framing;

/// <summary>
/// Splits on an arbitrary byte delimiter (single or multi-byte), excluded from the record.
/// The final record is emitted even without a trailing delimiter.
/// </summary>
public sealed class DelimiterFraming : IRecordFraming
{
    private readonly ReadOnlyMemory<byte> _delimiter;

    public DelimiterFraming(ReadOnlyMemory<byte> delimiter)
    {
        if (delimiter.IsEmpty)
            throw new ArgumentException("Delimiter must be non-empty.", nameof(delimiter));
        _delimiter = delimiter;
    }

    public DelimiterFraming(string delimiter) : this(Encoding.UTF8.GetBytes(delimiter)) { }

    public bool TryReadRecord(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> record)
    {
        var reader = new SequenceReader<byte>(buffer);
        if (reader.TryReadTo(out record, _delimiter.Span, advancePastDelimiter: true))
        {
            buffer = buffer.Slice(reader.Position);
            return true;
        }

        record = default;
        return false;
    }


}
