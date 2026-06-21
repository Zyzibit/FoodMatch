using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using foodmatch.ETL.Framing;
using foodmatch.ETL.Parsing;
using foodmatch.ETL.Resilience;
using foodmatch.ETL.Sources;

namespace foodmatch.ETL.Building;

/// <summary>Configured source(s) and options before a parser is chosen.</summary>
public sealed class PipelineSource
{
    private readonly IReadOnlyList<SourceEntry> _sources;
    private readonly PipelineOptions _options = new();
    private ICheckpointStore? _checkpoint;
    private string? _checkpointPath;

    internal PipelineSource(IReadOnlyList<SourceEntry> sources) => _sources = sources;

    public PipelineSource Configure(Action<PipelineOptions> configure)
    {
        configure(_options);
        return this;
    }

    /// <summary>Choose how the byte stream is split into records (default: one record per line).</summary>
    public PipelineSource Frame(IRecordFraming framing)
    {
        _options.Framing = framing;
        return this;
    }

    /// <summary>Persist per-file progress to <paramref name="checkpointPath"/> and skip done files on re-run.</summary>
    public PipelineSource WithCheckpoint(string checkpointPath)
    {
        _checkpointPath = checkpointPath;
        return this;
    }

    /// <summary>Use a custom <see cref="ICheckpointStore"/> for per-file resume.</summary>
    public PipelineSource WithCheckpoint(ICheckpointStore store)
    {
        _checkpoint = store;
        return this;
    }

    /// <summary>Parse each line as JSON into <typeparamref name="T"/> (from UTF-8 bytes, reflection-based).</summary>
    public DataPipelineBuilder<T> DeserializeJson<T>(JsonSerializerOptions? jsonOptions = null) where T : class =>
        Parse(new JsonRecordParser<T>(jsonOptions));

    /// <summary>Parse as JSON using source-gen metadata (<c>JsonSerializerContext</c>) — no reflection.</summary>
    public DataPipelineBuilder<T> DeserializeJson<T>(JsonTypeInfo<T> typeInfo) where T : class =>
        Parse(new JsonRecordParser<T>(typeInfo));

    /// <summary>Use a custom, hand-written parser (e.g. a <c>Utf8JsonReader</c> over fields).</summary>
    public DataPipelineBuilder<T> Parse<T>(IRecordParser<T> parser) where T : class
    {
        // Use Parse (not TryParse): malformed lines must throw so the worker can route them
        // through the configured ErrorPolicy instead of being silently dropped.
        RecordProjection<T> projection = line =>
            new ValueTask<ProjectionOutcome<T>>(
                parser.Parse(line.Span) is { } value
                    ? ProjectionOutcome<T>.Keep(value)
                    : ProjectionOutcome<T>.Drop);

        return new DataPipelineBuilder<T>(_sources, _options, projection, progress: null, _checkpoint, _checkpointPath);
    }
}
