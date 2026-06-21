using System.IO.Pipelines;

namespace inzynierka.ETL.Sources;

/// <summary>
/// Byte source for the pipeline. Abstracts over the concrete medium (a file, any
/// <see cref="Stream"/>, a ready <see cref="PipeReader"/>) so the engine is not tied to the file
/// system. Decompression (gzip etc.) is handled by wrapping the stream on the caller's side and
/// passing it to <c>DataPipeline.FromStream</c>. Implement this to add custom sources (S3, HTTP, …).
/// </summary>
public interface IDataSource
{
    /// <summary>Validation before the run (e.g. file existence). No-op by default.</summary>
    void Validate() { }

    /// <summary>
    /// Creates a <see cref="PipeReader"/> over the source. <paramref name="owned"/> returns the
    /// resource the engine must dispose when done, or <c>null</c> when the caller stays the owner.
    /// </summary>
    PipeReader OpenReader(int readBufferSize, out IAsyncDisposable? owned);
}
