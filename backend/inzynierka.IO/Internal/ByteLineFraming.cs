using System.Buffers;

namespace inzynierka.IO.Internal;

/// <summary>
/// Współdzielona logika podziału ciągłego strumienia bajtów na linie (framing po '\n')
/// na poziomie <see cref="ReadOnlySequence{T}"/> — bez alokowania buforów pośrednich.
/// Używana zarówno przez <see cref="PipelineFileReader"/>, jak i przez silnik potoku.
/// </summary>
internal static class ByteLineFraming
{
    /// <summary>
    /// Próbuje wyciąć jedną linię (do najbliższego '\n', bez delimitera) z bufora.
    /// Po sukcesie <paramref name="buffer"/> jest przesuwany za zakończenie linii.
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

    /// <summary>
    /// Czy linia jest pusta lub złożona wyłącznie z białych znaków ASCII (spacja/tab/CR).
    /// Odpowiednik <c>string.IsNullOrWhiteSpace</c> na poziomie bajtów.
    /// </summary>
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
