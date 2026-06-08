namespace FastPipe.Pipeline;

/// <summary>
/// Engine tuning. Sensible defaults — simple usage needs nothing set.
/// </summary>
public sealed class PipelineOptions
{
    /// <summary>Number of parallel parsing workers. Defaults to the processor count.</summary>
    public int Parallelism { get; set; } = Environment.ProcessorCount;

    /// <summary>Buffer capacity between stages (backpressure). Higher means a larger RAM buffer.</summary>
    public int ChannelCapacity { get; set; } = 10_000;

    /// <summary>Size of the batch handed to <see cref="IBatchSink{T}"/>.</summary>
    public int BatchSize { get; set; } = 500;

    /// <summary>Read buffer segment size for the source (bytes).</summary>
    public int ReadBufferSize { get; set; } = 1_048_576; // 1 MB

    /// <summary>Reaction to a record parse error.</summary>
    public ErrorPolicy ErrorPolicy { get; set; } = ErrorPolicy.Skip;

    /// <summary>Skip empty/whitespace lines without counting them as errors. Defaults to true.</summary>
    public bool SkipBlankLines { get; set; } = true;

    /// <summary>Strip the UTF-8 BOM from the first line. Defaults to true.</summary>
    public bool StripByteOrderMark { get; set; } = true;

    /// <summary>
    /// Remove NUL bytes (0x00) from every line before parsing. Defaults to true — dumps are
    /// sometimes polluted with NULs that break parsers. Set to <c>false</c> when you need a
    /// byte-for-byte faithful input.
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
