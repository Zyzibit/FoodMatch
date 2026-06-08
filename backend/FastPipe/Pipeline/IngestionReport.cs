namespace inzynierka.IO.Pipeline;

/// <summary>
/// Podsumowanie przebiegu potoku — twarde liczby do logów, metryk i benchmarków.
/// </summary>
public sealed record IngestionReport
{
    /// <summary>Wszystkie sframowane linie (łącznie z pustymi).</summary>
    public long LinesRead { get; init; }

    /// <summary>Puste/białe linie pominięte przed parsowaniem.</summary>
    public long BlankLinesSkipped { get; init; }

    /// <summary>Rekordy odrzucone przez parser (TryParse == false) lub przez filtr Where.</summary>
    public long Dropped { get; init; }

    /// <summary>Rekordy, które rzuciły wyjątek przy parsowaniu (przy ErrorPolicy.Skip).</summary>
    public long Failed { get; init; }

    /// <summary>Rekordy przekazane do sinka.</summary>
    public long ItemsWritten { get; init; }

    /// <summary>Liczba paczek wysłanych do sinka.</summary>
    public long BatchesWritten { get; init; }

    /// <summary>Czas całości.</summary>
    public TimeSpan Elapsed { get; init; }

    /// <summary>Przepustowość w liniach/sekundę.</summary>
    public double LinesPerSecond => Elapsed.TotalSeconds > 0 ? LinesRead / Elapsed.TotalSeconds : 0;

    public override string ToString() =>
        $"read={LinesRead}, written={ItemsWritten}, dropped={Dropped}, failed={Failed}, " +
        $"blank={BlankLinesSkipped}, batches={BatchesWritten}, elapsed={Elapsed.TotalSeconds:F2}s, " +
        $"throughput={LinesPerSecond:N0} lines/s";
}

/// <summary>
/// Migawka postępu raportowana w trakcie przez <see cref="IProgress{T}"/>.
/// </summary>
public readonly record struct PipelineProgress(long LinesRead, long ItemsWritten, long Failed);
