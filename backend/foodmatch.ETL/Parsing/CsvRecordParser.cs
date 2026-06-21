using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace foodmatch.ETL.Parsing;

/// <summary>
/// Parses a single CSV record (one already framed by <c>CsvFraming</c> or <c>LineFraming</c>) into
/// <typeparamref name="T"/>. Splits fields RFC-4180-style (separator, double-quotes, doubled-quote
/// escaping, optional trailing '\r'), then delegates building the record to a user-supplied mapper.
/// The mapper returns <c>null</c> to drop the row (e.g. a header line).
/// </summary>
public sealed class CsvRecordParser<T> : IRecordParser<T> where T : class
{
    private readonly Func<IReadOnlyList<string>, T?> _map;
    private readonly char _separator;

    public CsvRecordParser(Func<IReadOnlyList<string>, T?> map, char separator = ',')
    {
        _map = map;
        _separator = separator;
    }

    public T? Parse(ReadOnlySpan<byte> utf8Line)
    {
        var text = Encoding.UTF8.GetString(utf8Line);
        return _map(SplitFields(text, _separator));
    }

    public bool TryParse(ReadOnlySpan<byte> utf8Line, [MaybeNullWhen(false)] out T value)
    {
        try
        {
            if (Parse(utf8Line) is { } parsed)
            {
                value = parsed;
                return true;
            }
        }
        catch (FormatException)
        {
            // Malformed row — fall through to the dropped result below.
        }

        value = null;
        return false;
    }

    private static List<string> SplitFields(ReadOnlySpan<char> line, char separator)
    {
        if (line.Length > 0 && line[^1] == '\r')
            line = line[..^1];

        var fields = new List<string>();
        var sb = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"') { sb.Append('"'); i++; }
                    else inQuotes = false;
                }
                else sb.Append(c);
            }
            else if (c == '"') inQuotes = true;
            else if (c == separator) { fields.Add(sb.ToString()); sb.Clear(); }
            else sb.Append(c);
        }

        fields.Add(sb.ToString());
        return fields;
    }
}
