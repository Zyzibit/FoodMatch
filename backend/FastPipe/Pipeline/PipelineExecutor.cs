using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading.Channels;
using inzynierka.IO.Internal;

namespace inzynierka.IO.Pipeline;

/// <summary>
/// Silnik potoku ETL. Trzy etapy spięte ograniczonymi kanałami (backpressure):
///
///   [producent]  PipeReader czyta plik → framing linii → kopia do bufora z ArrayPool
///        │  (Channel&lt;LineSegment&gt;, bounded)
///   [N workerów] równoległe parsowanie z bajtów → projekcja (parse+Where+Select)
///        │  (Channel&lt;TOut&gt;, bounded)
///   [konsument]  batchowanie i zapis do IBatchSink (sekwencyjnie)
///
/// Pojedynczy producent, wielu workerów, pojedynczy konsument batchujący.
/// </summary>
internal static class PipelineExecutor
{
    private readonly record struct LineSegment(byte[] Buffer, int Length);

    private sealed class Counters
    {
        public long LinesRead;
        public long Blank;
        public long Dropped;
        public long Failed;
        public long Items;
        public long Batches;

        public PipelineProgress Snapshot() => new(
            Interlocked.Read(ref LinesRead),
            Interlocked.Read(ref Items),
            Interlocked.Read(ref Failed));
    }

    // ---- Tryb PUSH: pełny przebieg do sinka, zwraca raport -------------------

    public static async Task<IngestionReport> RunAsync<TOut>(
        PipelineDataSource source,
        RecordProjection<TOut> projection,
        IBatchSink<TOut> sink,
        PipelineOptions options,
        IProgress<PipelineProgress>? progress,
        CancellationToken externalCt)
    {
        options.Validate();
        var sw = Stopwatch.StartNew();
        var counters = new Counters();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(externalCt);
        var ct = cts.Token;
        ExceptionDispatchInfo? fatal = null;
        void OnFatal(Exception ex)
        {
            Interlocked.CompareExchange(ref fatal, ExceptionDispatchInfo.Capture(ex), null);
            cts.Cancel();
        }

        var (output, completion, producer) = StartCore(source, projection, options, counters, OnFatal, ct);

        // Konsument batchujący — sekwencyjny zapis do sinka.
        try
        {
            var batch = new List<TOut>(options.BatchSize);
            await foreach (var item in output.ReadAllAsync(ct))
            {
                batch.Add(item);
                if (batch.Count >= options.BatchSize)
                {
                    await FlushAsync(sink, batch, counters, progress, ct);
                    batch = new List<TOut>(options.BatchSize);
                }
            }
            if (batch.Count > 0)
                await FlushAsync(sink, batch, counters, progress, ct);
        }
        catch (OperationCanceledException) when (fatal is not null)
        {
            // Błąd krytyczny anulował potok — prawdziwy wyjątek rzucimy niżej.
        }
        finally
        {
            // Zatrzymaj producenta/workerów i poczekaj na nich, niezależnie od ścieżki
            // wyjścia (sukces, anulowanie, wyjątek sinka) — bez wycieku zadań i uchwytów.
            cts.Cancel();
            try { await producer; } catch { /* zgłoszone przez fatal lub nieistotne przy sprzątaniu */ }
            try { await completion; } catch { /* j.w. */ }
        }

        fatal?.Throw();

        sw.Stop();
        return new IngestionReport
        {
            LinesRead = counters.LinesRead,
            BlankLinesSkipped = counters.Blank,
            Dropped = counters.Dropped,
            Failed = counters.Failed,
            ItemsWritten = counters.Items,
            BatchesWritten = counters.Batches,
            Elapsed = sw.Elapsed
        };
    }

    private static async ValueTask FlushAsync<TOut>(
        IBatchSink<TOut> sink, List<TOut> batch, Counters counters,
        IProgress<PipelineProgress>? progress, CancellationToken ct)
    {
        await sink.WriteBatchAsync(batch, ct);
        Interlocked.Add(ref counters.Items, batch.Count);
        Interlocked.Increment(ref counters.Batches);
        progress?.Report(counters.Snapshot());
    }

    // ---- Tryb PULL: strumień wyjściowy jako IAsyncEnumerable -----------------

    public static async IAsyncEnumerable<TOut> StreamAsync<TOut>(
        PipelineDataSource source,
        RecordProjection<TOut> projection,
        PipelineOptions options,
        [EnumeratorCancellation] CancellationToken externalCt = default)
    {
        options.Validate();
        var counters = new Counters();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(externalCt);
        var ct = cts.Token;
        ExceptionDispatchInfo? fatal = null;
        void OnFatal(Exception ex)
        {
            Interlocked.CompareExchange(ref fatal, ExceptionDispatchInfo.Capture(ex), null);
            cts.Cancel();
        }

        var (output, completion, producer) = StartCore(source, projection, options, counters, OnFatal, ct);

        try
        {
            await foreach (var item in output.ReadAllAsync(ct))
            {
                Interlocked.Increment(ref counters.Items);
                yield return item;
            }
        }
        finally
        {
            // Także gdy konsument przerwie iterację (break/wyjątek): anuluj i domknij
            // producenta/workerów, by nie zawisły na zapisie do kanału ani nie trzymały pliku.
            cts.Cancel();
            try { await producer; } catch { /* nieistotne przy sprzątaniu */ }
            try { await completion; } catch { /* j.w. */ }
        }

        fatal?.Throw();
    }


    private static (ChannelReader<TOut> Output, Task Completion, Task Producer) StartCore<TOut>(
        PipelineDataSource source,
        RecordProjection<TOut> projection,
        PipelineOptions options,
        Counters counters,
        Action<Exception> onFatal,
        CancellationToken ct)
    {
        source.Validate();

        var lineChannel = Channel.CreateBounded<LineSegment>(new BoundedChannelOptions(options.ChannelCapacity)
        {
            SingleWriter = true,
            SingleReader = false,
            FullMode = BoundedChannelFullMode.Wait
        });

        var outputChannel = Channel.CreateBounded<TOut>(new BoundedChannelOptions(options.ChannelCapacity)
        {
            SingleWriter = false,
            SingleReader = true,
            FullMode = BoundedChannelFullMode.Wait
        });

        var producer = Task.Run(async () =>
        {
            try
            {
                await ProduceLinesAsync(source, options, lineChannel.Writer, counters, ct);
                lineChannel.Writer.Complete();
            }
            catch (Exception ex)
            {
                lineChannel.Writer.Complete(ex);
                if (ex is not OperationCanceledException) onFatal(ex);
            }
        }, ct);

        var workers = new Task[options.Parallelism];
        for (var i = 0; i < workers.Length; i++)
            workers[i] = Task.Run(() => RunWorkerAsync(
                lineChannel.Reader, outputChannel.Writer, projection, options.ErrorPolicy, counters, onFatal, ct), ct);

        var completion = Task.Run(async () =>
        {
            Exception? error = null;
            try { await Task.WhenAll(workers); }
            catch (Exception ex) { error = ex; }
            finally { outputChannel.Writer.Complete(error is OperationCanceledException ? null : error); }
        });

        return (outputChannel.Reader, completion, producer);
    }

    private static async Task ProduceLinesAsync(
        PipelineDataSource source, PipelineOptions options, ChannelWriter<LineSegment> writer, Counters counters, CancellationToken ct)
    {
        var reader = source.OpenReader(options.ReadBufferSize, out var owned);
        var firstLine = true;

        try
        {
            while (true)
            {
                var result = await reader.ReadAsync(ct);
                var buffer = result.Buffer;

                while (ByteLineFraming.TryReadLine(ref buffer, out var line))
                    await EmitLineAsync(in line, writer, options, counters, ref firstLine, ct);

                if (result.IsCompleted)
                {
                    if (!buffer.IsEmpty)
                        await EmitLineAsync(in buffer, writer, options, counters, ref firstLine, ct);
                    reader.AdvanceTo(buffer.End);
                    break;
                }

                reader.AdvanceTo(buffer.Start, buffer.End);
            }
        }
        finally
        {
            await reader.CompleteAsync();
            if (owned is not null) await owned.DisposeAsync();
        }
    }

    private static ValueTask EmitLineAsync(
        in ReadOnlySequence<byte> line, ChannelWriter<LineSegment> writer, PipelineOptions options,
        Counters counters, ref bool firstLine, CancellationToken ct)
    {
        Interlocked.Increment(ref counters.LinesRead);

        if (options.SkipBlankLines && ByteLineFraming.IsBlank(in line))
        {
            Interlocked.Increment(ref counters.Blank);
            return ValueTask.CompletedTask;
        }

        var segment = CopyLine(in line, ref firstLine, options.StripByteOrderMark, options.StripNullBytes);
        if (segment.Length == 0)
        {
            ArrayPool<byte>.Shared.Return(segment.Buffer);
            Interlocked.Increment(ref counters.Blank);
            return ValueTask.CompletedTask;
        }

        return writer.WriteAsync(segment, ct);
    }

    /// <summary>
    /// Kopiuje linię do bufora z puli, opcjonalnie zdejmując BOM (pierwsza linia) i bajty NUL.
    /// </summary>
    private static LineSegment CopyLine(in ReadOnlySequence<byte> line, ref bool firstLine, bool stripBom, bool stripNul)
    {
        var len = checked((int)line.Length);
        var rented = ArrayPool<byte>.Shared.Rent(len);
        line.CopyTo(rented);

        var start = 0;
        if (firstLine)
        {
            firstLine = false;
            if (stripBom && len >= 3 && rented[0] == 0xEF && rented[1] == 0xBB && rented[2] == 0xBF)
                start = 3;
        }

        // Nic do zdjęcia — oddaj linię bez kompaktowania.
        if (start == 0 && !stripNul)
            return new LineSegment(rented, len);

        // Kompaktowanie: zdejmij BOM (start>0) i — gdy włączone — usuń bajty NUL.
        var w = 0;
        for (var i = start; i < len; i++)
        {
            var b = rented[i];
            if (stripNul && b == 0) continue;
            rented[w++] = b;
        }

        return new LineSegment(rented, w);
    }

    private static async Task RunWorkerAsync<TOut>(
        ChannelReader<LineSegment> reader, ChannelWriter<TOut> output, RecordProjection<TOut> projection,
        ErrorPolicy policy, Counters counters, Action<Exception> onFatal, CancellationToken ct)
    {
        await foreach (var segment in reader.ReadAllAsync(ct))
        {
            TOut value;
            bool ok;
            try
            {
                try
                {
                    ok = projection(new ReadOnlyMemory<byte>(segment.Buffer, 0, segment.Length), out value);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(segment.Buffer);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                if (policy == ErrorPolicy.Throw)
                {
                    onFatal(ex);
                    throw;
                }
                Interlocked.Increment(ref counters.Failed);
                continue;
            }

            if (ok)
                await output.WriteAsync(value!, ct);
            else
                Interlocked.Increment(ref counters.Dropped);
        }
    }
}
