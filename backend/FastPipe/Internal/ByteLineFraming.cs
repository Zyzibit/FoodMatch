using System.Buffers;

namespace FastPipe.Internal;
internal static class ByteLineFraming
{
    /// <summary>
    /// Tries to cut one line (up to the next '\n', delimiter excluded) from the buffer.
    /// On success <paramref name="buffer"/> is advanced past the end of the line.
    /// </summary>
    public static bool TryReadLine(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line)
    {
        var seq = new SequenceReader<byte>(buffer);
        if (seq.TryReadTo(out line, (byte)'\n', advancePastDelimiter: true))
        {
            buffer = buffer.Slice(seq.Position);
            return true;
        }

        line = default;
        return false;
    }
    public static bool IsBlank(in ReadOnlySequence<byte> line)
    {
        foreach (var segment in line)
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
