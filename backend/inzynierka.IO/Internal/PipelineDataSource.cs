using System.IO.Pipelines;

namespace inzynierka.IO.Internal;

/// <summary>
/// Źródło bajtów dla potoku. Abstrahuje od konkretnego nośnika (plik, dowolny
/// <see cref="Stream"/>, gotowy <see cref="PipeReader"/>), dzięki czemu silnik nie
/// jest związany z systemem plików. Dekompresję (gzip itp.) realizuje się przez
/// owinięcie strumienia po stronie wołającego i podanie go do <c>FromStream</c>.
/// </summary>
internal abstract class PipelineDataSource
{
    /// <summary>Walidacja przed startem (np. istnienie pliku). Domyślnie brak.</summary>
    public virtual void Validate() { }

    /// <summary>
    /// Tworzy <see cref="PipeReader"/> do czytania źródła. W <paramref name="owned"/>
    /// zwraca zasób, który silnik ma zwolnić po zakończeniu (lub <c>null</c>, gdy
    /// właścicielem pozostaje wołający).
    /// </summary>
    public abstract PipeReader OpenReader(int readBufferSize, out IAsyncDisposable? owned);
}

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

    // Czytnik należy do wołającego — silnik go nie zwalnia (poza CompleteAsync na końcu odczytu).
    public override PipeReader OpenReader(int readBufferSize, out IAsyncDisposable? owned)
    {
        owned = null;
        return _reader;
    }
}
