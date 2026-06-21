using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading.Channels;
using foodmatch.ETL.Building;
using foodmatch.ETL.Diagnostics;
using foodmatch.ETL.Resilience;
using foodmatch.ETL.Sinks;
using foodmatch.ETL.Sources;

namespace foodmatch.ETL.Engine;

/// <summary>
/// ETL pipeline engine. Three stages connected by bounded channels (backpressure):
///
///   [producer]   PipeReader reads the source → record framing → copy into a pooled buffer
///        │  (Channel&lt;LineSegment&gt;, bounded)
///   [N workers]  parallel parse from bytes → projection (parse + Where + Select)
///        │  (Channel&lt;TOut&gt;, bounded)
///   [consumer]   batching and writing to IBatchSink (sequentially)
///
/// Single producer, many workers, single batching consumer.
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

    // ---- PUSH mode: full run into the sink, returns a report ------------------

    public static async Task<IngestionReport> RunAsync<TOut>(
        IDataSource source,
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

        // Batching consumer — sequential writes to the sink.
        try
        {
            var batch = new List<TOut>(options.BatchSize);
            await foreach (var item in output.ReadAllAsync(ct))
            {
                batch.Add(item);
                if (batch.Count >= options.BatchSize)
                {
                    await FlushAsync(sink, batch, counters, progress, ct);
                    batch.Clear();
                }
            }
            if (batch.Count > 0)
                await FlushAsync(sink, batch, counters, progress, ct);
        }
        catch (OperationCanceledException) when (fatal is not null)
        {
            // A fatal error cancelled the pipeline — the real exception is rethrown below.
        }
        finally
        {
            // Stop the producer/workers and await them regardless of the exit path
            // (success, cancellation, sink exception) — no leaked tasks or handles.
            cts.Cancel();
            try { await producer; } catch { /* surfaced via fatal, or irrelevant during cleanup */ }
            try { await completion; } catch { /* as above */ }
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

    // ---- PULL mode: output stream as IAsyncEnumerable ------------------------

    public static async IAsyncEnumerable<TOut> StreamAsync<TOut>(
        IDataSource source,
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
                yield return item;
        }
        finally
        {
            // Also when the consumer breaks out early (break/exception): cancel and drain
            // the producer/workers so they don't hang writing to the channel or hold the file.
            cts.Cancel();
            try { await producer; } catch { /* irrelevant during cleanup */ }
            try { await completion; } catch { /* as above */ }
        }

        fatal?.Throw();
    }


    private static (ChannelReader<TOut> Output, Task Completion, Task Producer) StartCore<TOut>(
        IDataSource source,
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
                lineChannel.Reader, outputChannel.Writer, projection, options, counters, onFatal, ct), ct);

        // No token: this task must always run to completion to close the output channel.
        var completion = Task.Run(async () =>
        {
            Exception? error = null;
            try { await Task.WhenAll(workers); }
            catch (Exception ex) { error = ex; }
            finally { outputChannel.Writer.Complete(error is OperationCanceledException ? null : error); }
        }, CancellationToken.None);

        return (outputChannel.Reader, completion, producer);
    }

    private static async Task ProduceLinesAsync(
        IDataSource source, PipelineOptions options, ChannelWriter<LineSegment> writer, Counters counters, CancellationToken ct)
    {
        var framing = options.Framing;
        var reader = source.OpenReader(options.ReadBufferSize, out var owned);
        var firstLine = true;

        try
        {
            while (true)
            {
                var result = await reader.ReadAsync(ct);
                var buffer = result.Buffer;

                while (framing.TryReadRecord(ref buffer, out var record))
                    await EmitLineAsync(in record, writer, options, counters, ref firstLine, ct);

                if (result.IsCompleted)
                {
                    if (framing.TryReadTrailing(in buffer, out var trailing))
                        await EmitLineAsync(in trailing, writer, options, counters, ref firstLine, ct);
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

        if (options.SkipBlankLines && options.Framing.IsBlank(in line))
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
    /// Copies the line into a pooled buffer, optionally stripping the BOM (first line) and NUL bytes.
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

        // Nothing to strip — hand the line over without compaction.
        if (start == 0 && !stripNul)
            return new LineSegment(rented, len);

        // Compaction: drop the BOM (start>0) and — when enabled — remove NUL bytes.
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
        PipelineOptions options, Counters counters, Action<Exception> onFatal, CancellationToken ct)
    {
        var policy = options.ErrorPolicy;
        var deadLetter = options.DeadLetterSink;

        await foreach (var segment in reader.ReadAllAsync(ct))
        {
            ProjectionOutcome<TOut> outcome;
            try
            {
                try
                {
                    // The projection parses synchronously and may then run an async transform.
                    // The pooled buffer is only touched during parsing; returning it after the
                    // projection completes is safe (the async map operates on the parsed value).
                    outcome = await projection(segment.Buffer.AsMemory(0, segment.Length));
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    if (policy == ErrorPolicy.Throw)
                    {
                        onFatal(ex);
                        throw;
                    }
                    if (policy == ErrorPolicy.DeadLetter && deadLetter is not null)
                    {
                        // Copy the raw bytes before the buffer returns to the pool (in finally).
                        var raw = segment.Buffer.AsSpan(0, segment.Length).ToArray();
                        await deadLetter.WriteAsync(raw, ex, ct);
                    }
                    Interlocked.Increment(ref counters.Failed);
                    continue;
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(segment.Buffer);
            }

            if (outcome.Produced)
                await output.WriteAsync(outcome.Value, ct);
            else
                Interlocked.Increment(ref counters.Dropped);
        }
    }
}
