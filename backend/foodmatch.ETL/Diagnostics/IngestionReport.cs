namespace foodmatch.ETL.Diagnostics;

/// <summary>
/// Summary of a pipeline run — hard numbers for logs, metrics and benchmarks.
/// </summary>
public sealed record IngestionReport
{
    /// <summary>All framed lines (including blank ones).</summary>
    public long LinesRead { get; init; }

    /// <summary>Empty/whitespace lines skipped before parsing.</summary>
    public long BlankLinesSkipped { get; init; }

    /// <summary>Records dropped by the parser (TryParse == false) or by a Where filter.</summary>
    public long Dropped { get; init; }

    /// <summary>Records that threw during parsing (under ErrorPolicy.Skip).</summary>
    public long Failed { get; init; }

    /// <summary>Records handed to the sink.</summary>
    public long ItemsWritten { get; init; }

    /// <summary>Number of batches sent to the sink.</summary>
    public long BatchesWritten { get; init; }

    /// <summary>Total elapsed time.</summary>
    public TimeSpan Elapsed { get; init; }

    /// <summary>Throughput in lines per second.</summary>
    public double LinesPerSecond => Elapsed.TotalSeconds > 0 ? LinesRead / Elapsed.TotalSeconds : 0;

    public static IngestionReport operator +(IngestionReport a, IngestionReport b) => new()
    {
        LinesRead = a.LinesRead + b.LinesRead,
        BlankLinesSkipped = a.BlankLinesSkipped + b.BlankLinesSkipped,
        Dropped = a.Dropped + b.Dropped,
        Failed = a.Failed + b.Failed,
        ItemsWritten = a.ItemsWritten + b.ItemsWritten,
        BatchesWritten = a.BatchesWritten + b.BatchesWritten,
        Elapsed = a.Elapsed + b.Elapsed,
    };

    public override string ToString() =>
        $"read={LinesRead}, written={ItemsWritten}, dropped={Dropped}, failed={Failed}, " +
        $"blank={BlankLinesSkipped}, batches={BatchesWritten}, elapsed={Elapsed.TotalSeconds:F2}s, " +
        $"throughput={LinesPerSecond:N0} lines/s";
}

/// <summary>
/// Progress snapshot reported during the run via <see cref="IProgress{T}"/>.
/// </summary>
public readonly record struct PipelineProgress(long LinesRead, long ItemsWritten, long Failed);
