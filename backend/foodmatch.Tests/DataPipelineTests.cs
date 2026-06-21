using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Text;
using System.Text.Json;
using foodmatch.ETL;
using foodmatch.ETL.Framing;
using foodmatch.ETL.Parsing;
using foodmatch.ETL.Resilience;
using foodmatch.ETL.Sinks;
using foodmatch.ETL.Sources;

namespace foodmatch.Tests.DataPipelineTests
{
    public class DataPipelineTests : IDisposable
    {
        private readonly List<string> _tempFiles = new();
        private readonly List<string> _tempDirs = new();

        private sealed record Item(int Id, string Name);

        private string WriteJsonl(IEnumerable<string> lines)
        {
            var path = Path.GetTempFileName();
            _tempFiles.Add(path);
            File.WriteAllText(path, string.Join('\n', lines));
            return path;
        }

        private static string Json(int id, string name) =>
            JsonSerializer.Serialize(new Item(id, name));

        private static (CollectingSink<T> sink, Func<IReadOnlyList<T>, CancellationToken, ValueTask> write) Collector<T>()
        {
            var sink = new CollectingSink<T>();
            return (sink, sink.WriteBatchAsync);
        }

        private sealed class CollectingSink<T> : IBatchSink<T>
        {
            public ConcurrentBag<T> Items { get; } = new();
            public List<int> BatchSizes { get; } = new();

            public ValueTask WriteBatchAsync(IReadOnlyList<T> batch, CancellationToken ct)
            {
                lock (BatchSizes) BatchSizes.Add(batch.Count);
                foreach (var i in batch) Items.Add(i);
                return ValueTask.CompletedTask;
            }
        }

        [Fact]
        public async Task Pipeline_DeserializesAndWritesAllRecords()
        {
            var path = WriteJsonl(Enumerable.Range(1, 1000).Select(i => Json(i, $"name{i}")));
            var (sink, write) = Collector<Item>();

            var report = await DataPipeline
                .FromJsonlFile(path)
                .DeserializeJson<Item>()
                .WithBatchSize(100)
                .WithParallelism(4)
                .WriteBatchesTo(write);

            Assert.Equal(1000, report.ItemsWritten);
            Assert.Equal(1000, sink.Items.Count);
            Assert.Equal(Enumerable.Range(1, 1000).ToHashSet(), sink.Items.Select(i => i.Id).ToHashSet());
            Assert.All(sink.BatchSizes, size => Assert.True(size <= 100));
        }

        [Fact]
        public async Task Pipeline_SkipsBlankLines()
        {
            var path = WriteJsonl(new[] { Json(1, "a"), "", "   ", Json(2, "b") });
            var (sink, write) = Collector<Item>();

            var report = await DataPipeline.FromJsonlFile(path).DeserializeJson<Item>().WriteBatchesTo(write);

            Assert.Equal(4, report.LinesRead);
            Assert.Equal(2, report.BlankLinesSkipped);
            Assert.Equal(2, sink.Items.Count);
        }

        [Fact]
        public async Task Pipeline_Where_DropsNonMatching()
        {
            var path = WriteJsonl(Enumerable.Range(1, 10).Select(i => Json(i, $"n{i}")));
            var (sink, write) = Collector<Item>();

            var report = await DataPipeline
                .FromJsonlFile(path)
                .DeserializeJson<Item>()
                .Where(i => i.Id % 2 == 0)
                .WriteBatchesTo(write);

            Assert.Equal(5, report.ItemsWritten);
            Assert.Equal(5, report.Dropped);
            Assert.All(sink.Items, i => Assert.Equal(0, i.Id % 2));
        }

        [Fact]
        public async Task Pipeline_Select_MapsType()
        {
            var path = WriteJsonl(Enumerable.Range(1, 10).Select(i => Json(i, $"n{i}")));
            var (sink, write) = Collector<int>();

            await DataPipeline
                .FromJsonlFile(path)
                .DeserializeJson<Item>()
                .Select(i => i.Id * 10)
                .WriteBatchesTo(write);

            Assert.Equal(Enumerable.Range(1, 10).Select(i => i * 10).ToHashSet(), sink.Items.ToHashSet());
        }

        [Fact]
        public async Task Pipeline_ErrorPolicySkip_CountsFailuresAndContinues()
        {
            var path = WriteJsonl(new[] { Json(1, "a"), "{ this is not json", Json(2, "b") });
            var (sink, write) = Collector<Item>();

            var report = await DataPipeline
                .FromJsonlFile(path)
                .DeserializeJson<Item>()
                .OnError(ErrorPolicy.Skip)
                .WriteBatchesTo(write);

            Assert.Equal(1, report.Failed);
            Assert.Equal(2, report.ItemsWritten);
            Assert.Equal(2, sink.Items.Count);
        }

        [Fact]
        public async Task Pipeline_ErrorPolicyThrow_PropagatesException()
        {
            var path = WriteJsonl(new[] { Json(1, "a"), "{ broken", Json(2, "b") });
            var (_, write) = Collector<Item>();

            await Assert.ThrowsAnyAsync<JsonException>(async () =>
                await DataPipeline
                    .FromJsonlFile(path)
                    .DeserializeJson<Item>()
                    .OnError(ErrorPolicy.Throw)
                    .WriteBatchesTo(write));
        }

        [Fact]
        public async Task Pipeline_StreamMode_YieldsAllRecords()
        {
            var path = WriteJsonl(Enumerable.Range(1, 500).Select(i => Json(i, $"n{i}")));

            var ids = new List<int>();
            await foreach (var item in DataPipeline.FromJsonlFile(path).DeserializeJson<Item>().StreamAsync())
                ids.Add(item.Id);

            Assert.Equal(500, ids.Count);
            Assert.Equal(Enumerable.Range(1, 500).ToHashSet(), ids.ToHashSet());
        }

        [Fact]
        public async Task Pipeline_HandlesBomAndCrlf()
        {
            var path = Path.GetTempFileName();
            _tempFiles.Add(path);
            var content = Json(1, "a") + "\r\n" + Json(2, "b") + "\r\n";
            var bytes = new byte[] { 0xEF, 0xBB, 0xBF }.Concat(System.Text.Encoding.UTF8.GetBytes(content)).ToArray();
            File.WriteAllBytes(path, bytes);
            var (sink, write) = Collector<Item>();

            var report = await DataPipeline.FromJsonlFile(path).DeserializeJson<Item>().WriteBatchesTo(write);

            Assert.Equal(0, report.Failed);
            Assert.Equal(2, sink.Items.Count);
        }

        [Fact]
        public async Task Pipeline_MissingFile_Throws()
        {
            var missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".jsonl");
            var (_, write) = Collector<Item>();

            await Assert.ThrowsAsync<FileNotFoundException>(async () =>
                await DataPipeline.FromJsonlFile(missing).DeserializeJson<Item>().WriteBatchesTo(write));
        }

        [Fact]
        public async Task Pipeline_SelectAsync_MapsType()
        {
            var path = WriteJsonl(Enumerable.Range(1, 10).Select(i => Json(i, $"n{i}")));
            var (sink, write) = Collector<int>();

            await DataPipeline
                .FromJsonlFile(path)
                .DeserializeJson<Item>()
                .SelectAsync(async i => { await Task.Yield(); return i.Id * 10; })
                .WriteBatchesTo(write);

            Assert.Equal(Enumerable.Range(1, 10).Select(i => i * 10).ToHashSet(), sink.Items.ToHashSet());
        }

        [Fact]
        public async Task Pipeline_WhereAsync_DropsNonMatching()
        {
            var path = WriteJsonl(Enumerable.Range(1, 10).Select(i => Json(i, $"n{i}")));
            var (sink, write) = Collector<Item>();

            var report = await DataPipeline
                .FromJsonlFile(path)
                .DeserializeJson<Item>()
                .WhereAsync(async i => { await Task.Yield(); return i.Id % 2 == 0; })
                .WriteBatchesTo(write);

            Assert.Equal(5, report.ItemsWritten);
            Assert.Equal(5, report.Dropped);
            Assert.All(sink.Items, i => Assert.Equal(0, i.Id % 2));
        }

        [Fact]
        public async Task Pipeline_FromStream_ReadsAllRecords()
        {
            var path = WriteJsonl(Enumerable.Range(1, 200).Select(i => Json(i, $"n{i}")));
            var (sink, write) = Collector<Item>();

            await using var fs = File.OpenRead(path);
            var report = await DataPipeline.FromStream(fs).DeserializeJson<Item>().WriteBatchesTo(write);

            Assert.Equal(200, report.ItemsWritten);
            Assert.Equal(200, sink.Items.Count);
        }

        [Fact]
        public async Task Pipeline_FromGZipStream_ReadsAllRecords()
        {
            var raw = string.Join('\n', Enumerable.Range(1, 150).Select(i => Json(i, $"n{i}")));
            var gzPath = Path.GetTempFileName();
            _tempFiles.Add(gzPath);
            await using (var file = File.Create(gzPath))
            await using (var gz = new System.IO.Compression.GZipStream(file, System.IO.Compression.CompressionLevel.Fastest))
                await gz.WriteAsync(System.Text.Encoding.UTF8.GetBytes(raw));

            var (sink, write) = Collector<Item>();
            await using var read = File.OpenRead(gzPath);
            await using var decompress = new System.IO.Compression.GZipStream(read, System.IO.Compression.CompressionMode.Decompress);

            var report = await DataPipeline.FromStream(decompress).DeserializeJson<Item>().WriteBatchesTo(write);

            Assert.Equal(150, report.ItemsWritten);
            Assert.Equal(Enumerable.Range(1, 150).ToHashSet(), sink.Items.Select(i => i.Id).ToHashSet());
        }

        [Fact]
        public async Task Pipeline_FromFiles_ProcessesAllFiles()
        {
            var dir = NewTempDir();
            WriteJsonlTo(Path.Combine(dir, "a.jsonl"), Enumerable.Range(1, 50).Select(i => Json(i, $"a{i}")));
            WriteJsonlTo(Path.Combine(dir, "b.jsonl"), Enumerable.Range(51, 50).Select(i => Json(i, $"b{i}")));

            var (sink, write) = Collector<Item>();
            var report = await DataPipeline.FromFiles(Path.Combine(dir, "*.jsonl")).DeserializeJson<Item>().WriteBatchesTo(write);

            Assert.Equal(100, report.ItemsWritten);
            Assert.Equal(Enumerable.Range(1, 100).ToHashSet(), sink.Items.Select(i => i.Id).ToHashSet());
        }

        [Fact]
        public async Task Pipeline_Checkpoint_SkipsCompletedFilesOnRerun()
        {
            var dir = NewTempDir();
            WriteJsonlTo(Path.Combine(dir, "a.jsonl"), Enumerable.Range(1, 10).Select(i => Json(i, $"a{i}")));
            WriteJsonlTo(Path.Combine(dir, "b.jsonl"), Enumerable.Range(11, 10).Select(i => Json(i, $"b{i}")));
            var checkpoint = Path.Combine(dir, "state.json");

            // First run imports everything and records both files as done.
            var (sink1, write1) = Collector<Item>();
            var first = await DataPipeline.FromFiles(Path.Combine(dir, "*.jsonl"))
                .WithCheckpoint(checkpoint)
                .DeserializeJson<Item>()
                .WriteBatchesTo(write1);
            Assert.Equal(20, first.ItemsWritten);

            // Second run with the same checkpoint skips both files — nothing re-imported.
            var (sink2, write2) = Collector<Item>();
            var second = await DataPipeline.FromFiles(Path.Combine(dir, "*.jsonl"))
                .WithCheckpoint(checkpoint)
                .DeserializeJson<Item>()
                .WriteBatchesTo(write2);
            Assert.Equal(0, second.ItemsWritten);
            Assert.Empty(sink2.Items);
        }

        // ---- Framing seams ------------------------------------------------------

        private sealed record Row(string Id, string Name);

        [Fact]
        public async Task Csv_Framing_HandlesQuotedMultilineField()
        {
            // Row 1's name spans two physical lines inside quotes; the header is dropped by the mapper.
            var csv = "id,name\n1,\"hello\nworld\"\n2,plain\n";
            var path = Path.GetTempFileName();
            _tempFiles.Add(path);
            File.WriteAllText(path, csv);
            var (sink, write) = Collector<Row>();

            var report = await DataPipeline.FromJsonlFile(path)
                .Frame(new CsvFraming())
                .Parse(new CsvRecordParser<Row>(f => f[0] == "id" ? null : new Row(f[0], f[1])))
                .WriteBatchesTo(write);

            Assert.Equal(2, report.ItemsWritten);
            Assert.Contains(sink.Items, r => r.Id == "1" && r.Name == "hello\nworld");
            Assert.Contains(sink.Items, r => r.Id == "2" && r.Name == "plain");
        }

        [Fact]
        public async Task Delimiter_Framing_SplitsOnCustomSeparator()
        {
            // Three JSON records on a single physical line separated by "||".
            var content = string.Join("||", Enumerable.Range(1, 3).Select(i => Json(i, $"n{i}")));
            var path = Path.GetTempFileName();
            _tempFiles.Add(path);
            File.WriteAllText(path, content);
            var (sink, write) = Collector<Item>();

            var report = await DataPipeline.FromJsonlFile(path)
                .Frame(new DelimiterFraming("||"))
                .DeserializeJson<Item>()
                .WriteBatchesTo(write);

            Assert.Equal(3, report.ItemsWritten);
            Assert.Equal(new[] { 1, 2, 3 }.ToHashSet(), sink.Items.Select(i => i.Id).ToHashSet());
        }

        // ---- Dead-letter --------------------------------------------------------

        private sealed class CollectingDeadLetter : IDeadLetterSink
        {
            public ConcurrentBag<string> Records { get; } = new();

            public ValueTask WriteAsync(ReadOnlyMemory<byte> rawRecord, Exception error, CancellationToken ct)
            {
                Records.Add(Encoding.UTF8.GetString(rawRecord.Span));
                return ValueTask.CompletedTask;
            }
        }

        [Fact]
        public async Task DeadLetter_RoutesMalformedRecordsAndCountsFailed()
        {
            var path = WriteJsonl(new[] { Json(1, "a"), "{ not json", Json(2, "b") });
            var (sink, write) = Collector<Item>();
            var dead = new CollectingDeadLetter();

            var report = await DataPipeline.FromJsonlFile(path)
                .DeserializeJson<Item>()
                .OnErrorDeadLetter(dead)
                .WriteBatchesTo(write);

            Assert.Equal(2, report.ItemsWritten);
            Assert.Equal(1, report.Failed);
            Assert.Single(dead.Records);
            Assert.Contains("not json", dead.Records.Single());
        }

        // ---- Sink decorators ----------------------------------------------------

        private sealed class FlakyBatchSink<T> : IBatchSink<T>
        {
            private readonly IBatchSink<T> _inner;
            private int _failuresLeft;
            public FlakyBatchSink(IBatchSink<T> inner, int failures) { _inner = inner; _failuresLeft = failures; }

            public ValueTask WriteBatchAsync(IReadOnlyList<T> batch, CancellationToken ct)
            {
                if (_failuresLeft-- > 0) throw new InvalidOperationException("transient");
                return _inner.WriteBatchAsync(batch, ct);
            }
        }

        [Fact]
        public async Task RetryBatchSink_RetriesTransientFailures()
        {
            var path = WriteJsonl(Enumerable.Range(1, 20).Select(i => Json(i, $"n{i}")));
            var collecting = new CollectingSink<Item>();
            var flaky = new FlakyBatchSink<Item>(collecting, failures: 2);
            var sink = new RetryBatchSink<Item>(flaky, maxRetries: 3, backoff: TimeSpan.FromMilliseconds(1));

            var report = await DataPipeline.FromJsonlFile(path)
                .DeserializeJson<Item>()
                .WithBatchSize(20)
                .WithParallelism(1)
                .WriteBatchesTo(sink);

            Assert.Equal(20, report.ItemsWritten);
            Assert.Equal(20, collecting.Items.Count);
        }

        [Fact]
        public async Task TeeBatchSink_FansOutToAllSinks()
        {
            var path = WriteJsonl(Enumerable.Range(1, 30).Select(i => Json(i, $"n{i}")));
            var a = new CollectingSink<Item>();
            var b = new CollectingSink<Item>();
            var sink = new TeeBatchSink<Item>(a, b);

            await DataPipeline.FromJsonlFile(path).DeserializeJson<Item>().WriteBatchesTo(sink);

            Assert.Equal(30, a.Items.Count);
            Assert.Equal(30, b.Items.Count);
        }

        // ---- Custom source ------------------------------------------------------

        private sealed class MemoryDataSource : IDataSource
        {
            private readonly byte[] _bytes;
            public MemoryDataSource(string content) => _bytes = Encoding.UTF8.GetBytes(content);

            public PipeReader OpenReader(int readBufferSize, out IAsyncDisposable? owned)
            {
                var stream = new MemoryStream(_bytes, writable: false);
                owned = stream;
                return PipeReader.Create(stream, new StreamPipeReaderOptions(bufferSize: readBufferSize, leaveOpen: true));
            }
        }

        [Fact]
        public async Task From_CustomDataSource_ReadsAllRecords()
        {
            var content = string.Join('\n', Enumerable.Range(1, 40).Select(i => Json(i, $"n{i}")));
            var (sink, write) = Collector<Item>();

            var report = await DataPipeline.From(new MemoryDataSource(content)).DeserializeJson<Item>().WriteBatchesTo(write);

            Assert.Equal(40, report.ItemsWritten);
            Assert.Equal(Enumerable.Range(1, 40).ToHashSet(), sink.Items.Select(i => i.Id).ToHashSet());
        }

        private string NewTempDir()
        {
            var dir = Path.Combine(Path.GetTempPath(), "fastpipe_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            _tempDirs.Add(dir);
            return dir;
        }

        private static void WriteJsonlTo(string path, IEnumerable<string> lines) =>
            File.WriteAllText(path, string.Join('\n', lines));

        public void Dispose()
        {
            foreach (var path in _tempFiles)
            {
                try { File.Delete(path); } catch { /* best effort */ }
            }
            foreach (var dir in _tempDirs)
            {
                try { Directory.Delete(dir, recursive: true); } catch { /* best effort */ }
            }
        }
    }
}
