using System.IO.Pipelines;

namespace foodmatch.ETL.Sources;

/// <summary>Reads from a ready <see cref="PipeReader"/>; the caller stays the owner.</summary>
public sealed class PipeReaderDataSource(PipeReader reader) : IDataSource
{
    // The reader belongs to the caller — the engine does not dispose it (only CompleteAsync at end of read).
    public PipeReader OpenReader(int readBufferSize, out IAsyncDisposable? owned)
    {
        owned = null;
        return reader;
    }
}
