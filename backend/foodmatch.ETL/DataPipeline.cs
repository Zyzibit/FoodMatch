using System.IO.Pipelines;
using foodmatch.ETL.Building;
using foodmatch.ETL.Sources;

namespace foodmatch.ETL;

/// <summary>
/// Entry point of the fluent API.
///
/// <code>
/// var report = await DataPipeline
///     .FromJsonlFile(path)
///     .DeserializeJson&lt;Product&gt;()
///     .Where(p =&gt; p.Code is not null)
///     .WithParallelism(Environment.ProcessorCount)
///     .WithBatchSize(500)
///     .OnError(ErrorPolicy.Skip)
///     .WriteBatchesTo(sink, ct);
/// </code>
///
/// The source can be a file, any <see cref="Stream"/> (including one wrapped in
/// <c>GZipStream</c>/a network stream), a ready <see cref="PipeReader"/>, a custom
/// <see cref="IDataSource"/>, or many files with a per-file checkpoint.
/// </summary>
public static class DataPipeline
{
    /// <summary>Read a single JSONL/line-delimited file from the file system.</summary>
    public static PipelineSource FromJsonlFile(string filePath) =>
        new([new SourceEntry(Path.GetFullPath(filePath), new FileDataSource(filePath))]);

    /// <summary>
    /// Read from any stream. For compressed data wrap the source, e.g.
    /// <c>FromStream(new GZipStream(File.OpenRead(path), CompressionMode.Decompress))</c>.
    /// </summary>
    public static PipelineSource FromStream(Stream stream, bool leaveOpen = false) =>
        new([new SourceEntry(null, new StreamDataSource(stream, leaveOpen))]);

    /// <summary>Read from a ready <see cref="PipeReader"/> (the caller stays the owner).</summary>
    public static PipelineSource FromPipeReader(PipeReader reader) =>
        new([new SourceEntry(null, new PipeReaderDataSource(reader))]);

    /// <summary>Read from a custom <see cref="IDataSource"/>.</summary>
    public static PipelineSource From(IDataSource source) =>
        new([new SourceEntry(null, source)]);

    /// <summary>
    /// Read many files matching a glob (e.g. <c>"dump/*.jsonl"</c>) or all files in a directory.
    /// Files are processed sequentially in ordinal path order. Combine with
    /// <see cref="DataPipelineBuilder{T}.WithCheckpoint(string)"/> to skip already-imported files on a re-run.
    /// </summary>
    public static PipelineSource FromFiles(string pathOrGlob) => FromFiles(ExpandFiles(pathOrGlob));

    /// <summary>Read a fixed set of files, processed sequentially in ordinal path order.</summary>
    public static PipelineSource FromFiles(IEnumerable<string> files)
    {
        var entries = files
            .Select(f => Path.GetFullPath(f))
            .OrderBy(p => p, StringComparer.Ordinal)
            .Select(p => new SourceEntry(p, (IDataSource)new FileDataSource(p)))
            .ToList();

        if (entries.Count == 0)
            throw new ArgumentException("No files matched.", nameof(files));

        return new PipelineSource(entries);
    }

    private static IEnumerable<string> ExpandFiles(string pathOrGlob)
    {
        if (Directory.Exists(pathOrGlob))
            return Directory.EnumerateFiles(pathOrGlob);

        var dir = Path.GetDirectoryName(pathOrGlob);
        var pattern = Path.GetFileName(pathOrGlob);
        return Directory.EnumerateFiles(string.IsNullOrEmpty(dir) ? "." : dir, pattern);
    }
}
