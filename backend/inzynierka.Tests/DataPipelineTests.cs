using System.Collections.Concurrent;
using System.Text.Json;
using inzynierka.IO.Pipeline;

namespace inzynierka.Tests.DataPipelineTests
{
    public class DataPipelineTests : IDisposable
    {
        private readonly List<string> _tempFiles = new();

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

        private sealed class CollectingSink<T>
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

        public void Dispose()
        {
            foreach (var path in _tempFiles)
            {
                try { File.Delete(path); } catch { /* best effort */ }
            }
        }
    }
}
