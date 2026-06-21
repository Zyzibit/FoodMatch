using System.IO.Pipelines;

namespace inzynierka.ETL.Sources;

/// <summary>
/// Reads from any <see cref="Stream"/>. Wrap the stream to decompress, e.g.
/// <c>new GZipStream(File.OpenRead(path), CompressionMode.Decompress)</c>.
/// </summary>
public sealed class StreamDataSource(Stream stream, bool leaveOpen) : IDataSource
{
    public PipeReader OpenReader(int readBufferSize, out IAsyncDisposable? owned)
    {
        owned = leaveOpen ? null : stream;
        return PipeReader.Create(stream, new StreamPipeReaderOptions(bufferSize: readBufferSize, leaveOpen: true));
    }
}
