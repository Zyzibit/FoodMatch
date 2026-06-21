using System.Text.Json;

namespace inzynierka.ETL.Resilience;

/// <summary>
/// JSON-file <see cref="ICheckpointStore"/>. Persists the set of completed source keys to a small
/// file; a source is recorded only after its whole run completed, so a crash mid-source leaves it
/// unmarked and it is retried. Marking is sequential (the runner processes one source at a time),
/// so no locking is required.
/// </summary>
public sealed class FileCheckpointStore : ICheckpointStore
{
    private sealed class State
    {
        public IEnumerable<string> Done { get; set; } = [];
    }

    private static readonly JsonSerializerOptions WriteOptions = new() { WriteIndented = true };

    private readonly string _path;
    private readonly HashSet<string> _done;

    private FileCheckpointStore(string path, HashSet<string> done)
    {
        _path = path;
        _done = done;
    }

    /// <summary>Loads an existing checkpoint file, or starts empty if it does not exist.</summary>
    public static async Task<FileCheckpointStore> LoadAsync(string path, CancellationToken ct = default)
    {
        var done = new HashSet<string>(StringComparer.Ordinal);
        if (File.Exists(path))
        {
            await using var fs = File.OpenRead(path);
            var state = await JsonSerializer.DeserializeAsync<State>(fs, cancellationToken: ct);
            if (state?.Done is not null)
                done.UnionWith(state.Done);
        }
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        return new FileCheckpointStore(path, done);
    }

    public bool IsDone(string key) => _done.Contains(key);

    public async Task MarkDoneAsync(string key, CancellationToken ct = default)
    {
        if (!_done.Add(key))
            return;

        var state = new State { Done = _done };
        var tmp = _path + ".tmp";

        await using (var fs = File.Create(tmp))
            await JsonSerializer.SerializeAsync(fs, state, WriteOptions, ct);

        // Atomic replace so a crash never leaves a half-written checkpoint.
        File.Move(tmp, _path, overwrite: true);
    }
}
