using System.Buffers;

namespace inzynierka.ETL.Framing;

/// <summary>
/// RFC-4180 record framing: one record per line, but a newline inside a double-quoted field is
/// part of the record (multiline records). Quotes are escaped by doubling (<c>""</c>). The record
/// slice includes its quoting and any trailing '\r'; field parsing is the parser's job.
/// </summary>
public sealed class CsvFraming : IRecordFraming
{
    public bool TryReadRecord(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> record)
    {
        var reader = new SequenceReader<byte>(buffer);
        var inQuotes = false;

        while (reader.TryRead(out var b))
        {
            if (b == (byte)'"')
            {
                // A doubled quote inside a quoted field is an escaped quote — skip the pair.
                if (inQuotes && reader.TryPeek(out var next) && next == (byte)'"')
                {
                    reader.Advance(1);
                    continue;
                }
                inQuotes = !inQuotes;
            }
            else if (b == (byte)'\n' && !inQuotes)
            {
                var newlineOffset = reader.Consumed - 1; // index of the '\n'
                record = buffer.Slice(0, newlineOffset);
                buffer = buffer.Slice(reader.Consumed);
                return true;
            }
        }

        record = default;
        return false; // need more data (or an unterminated final record handled by TryReadTrailing)
    }


}
