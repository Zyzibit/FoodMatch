using System.IO.Pipelines;

namespace FastPipe.Internal;

/// <summary>
/// Byte source for the pipeline. Abstracts over the concrete medium (a file, any
/// <see cref="Stream"/>, a ready <see cref="PipeReader"/>) so the engine is not tied to the
/// file system. Decompression (gzip etc.) is handled by wrapping the stream on the caller's
/// side and passing it to <c>FromStream</c>.
/// </summary>
internal abstract class PipelineDataSource
{
    /// <summary>Validation before the run (e.g. file existence). No-op by default.</summary>
    public virtual void Validate() { }

    /// <summary>
    /// Creates a <see cref="PipeReader"/> over the source. <paramref name="owned"/> returns the
    /// resource the engine must dispose when done, or <c>null</c> when the caller stays the owner.
    /// </summary>
    public abstract PipeReader OpenReader(int readBufferSize, out IAsyncDisposable? owned);
}

/// <summary>
/// A data source paired with an optional stable key. The key (an absolute file path for file
/// sources) identifies the source for checkpointing; <c>null</c> means the source is not
/// checkpointable (e.g. a raw stream or pipe reader).
/// </summary>
internal readonly record struct SourceEntry(string? Key, PipelineDataSource Source);

internal sealed class FilePipelineSource(string path) : PipelineDataSource
{
    private readonly string _path = path;

    public override void Validate()
    {
        if (!File.Exists(_path))
            throw new FileNotFoundException($"File not found: {_path}", _path);
    }

    public override PipeReader OpenReader(int readBufferSize, out IAsyncDisposable? owned)
    {
        var stream = new FileStream(_path, new FileStreamOptions
        {
            Mode = FileMode.Open,
            Access = FileAccess.Read,
            Share = FileShare.Read,
            BufferSize = 0,
            Options = FileOptions.Asynchronous | FileOptions.SequentialScan
        });
        owned = stream;
        return PipeReader.Create(stream, new StreamPipeReaderOptions(bufferSize: readBufferSize, leaveOpen: true));
    }
}

internal sealed class StreamPipelineSource(Stream stream, bool leaveOpen) : PipelineDataSource
{
    private readonly Stream _stream = stream;
    private readonly bool _leaveOpen = leaveOpen;

    public override PipeReader OpenReader(int readBufferSize, out IAsyncDisposable? owned)
    {
        owned = _leaveOpen ? null : _stream;
        return PipeReader.Create(_stream, new StreamPipeReaderOptions(bufferSize: readBufferSize, leaveOpen: true));
    }
}

internal sealed class PipeReaderPipelineSource(PipeReader reader) : PipelineDataSource
{
    private readonly PipeReader _reader = reader;

    // The reader belongs to the caller — the engine does not dispose it (only CompleteAsync at end of read).
    public override PipeReader OpenReader(int readBufferSize, out IAsyncDisposable? owned)
    {
        owned = null;
        return _reader;
    }
}
