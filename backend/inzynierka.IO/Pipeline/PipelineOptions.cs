namespace inzynierka.IO.Pipeline;

/// <summary>
/// Strojenie silnika potoku. Sensowne domyślne wartości — w prostym użyciu
/// nie trzeba ustawiać niczego.
/// </summary>
public sealed class PipelineOptions
{
    /// <summary>Liczba równoległych workerów parsujących. Domyślnie = liczba rdzeni.</summary>
    public int Parallelism { get; set; } = Environment.ProcessorCount;

    /// <summary>Pojemność bufora między etapami (backpressure). Im więcej, tym większy bufor RAM.</summary>
    public int ChannelCapacity { get; set; } = 10_000;

    /// <summary>Rozmiar paczki przekazywanej do <see cref="IBatchSink{T}"/>.</summary>
    public int BatchSize { get; set; } = 500;

    /// <summary>Rozmiar segmentu bufora czytania pliku (bajty).</summary>
    public int ReadBufferSize { get; set; } = 1_048_576; // 1 MB

    /// <summary>Reakcja na błąd parsowania rekordu.</summary>
    public ErrorPolicy ErrorPolicy { get; set; } = ErrorPolicy.Skip;

    /// <summary>Pomijaj puste/białe linie bez liczenia ich jako błąd. Domyślnie tak.</summary>
    public bool SkipBlankLines { get; set; } = true;

    /// <summary>Zdejmij UTF-8 BOM z pierwszej linii. Domyślnie tak.</summary>
    public bool StripByteOrderMark { get; set; } = true;

    /// <summary>
    /// Usuwaj bajty NUL (0x00) z każdej linii przed parsowaniem. Domyślnie tak —
    /// dumpy bywają zanieczyszczone NUL-ami, które wywracają parsery. Ustaw na
    /// <c>false</c>, gdy potrzebujesz bajt-w-bajt wiernego wejścia.
    /// </summary>
    public bool StripNullBytes { get; set; } = true;

    internal void Validate()
    {
        if (Parallelism < 1) throw new ArgumentOutOfRangeException(nameof(Parallelism));
        if (ChannelCapacity < 1) throw new ArgumentOutOfRangeException(nameof(ChannelCapacity));
        if (BatchSize < 1) throw new ArgumentOutOfRangeException(nameof(BatchSize));
        if (ReadBufferSize < 4096) throw new ArgumentOutOfRangeException(nameof(ReadBufferSize));
    }
}
