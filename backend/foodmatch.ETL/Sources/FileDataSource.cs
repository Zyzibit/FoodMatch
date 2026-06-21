using System.IO.Pipelines;

namespace inzynierka.ETL.Sources;

/// <summary>Reads a file from the file system with sequential-scan, async I/O.</summary>
public sealed class FileDataSource(string path) : IDataSource
{
    public void Validate()
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"File not found: {path}", path);
    }

    public PipeReader OpenReader(int readBufferSize, out IAsyncDisposable? owned)
    {
        var stream = new FileStream(path, new FileStreamOptions
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
