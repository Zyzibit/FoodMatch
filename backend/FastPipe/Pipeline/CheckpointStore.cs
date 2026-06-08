using System.Text.Json;

namespace FastPipe.Pipeline;

/// <summary>
/// File-granularity checkpoint for multi-file runs. Persists the set of completed source
/// keys (absolute file paths) to a small JSON file. On a re-run, sources already marked as
/// done are skipped, so an interrupted import resumes from the first unprocessed file.
///
/// A file is recorded only after its whole pipeline run completed successfully, so a crash
/// mid-file leaves it unmarked and it is retried on the next run. Marking is sequential
/// (the runner processes one source at a time), so no locking is required.
/// </summary>
public sealed class CheckpointStore
{
    private sealed class State
    {
        public List<string> Done { get; set; } = [];
    }

    private static readonly JsonSerializerOptions WriteOptions = new() { WriteIndented = true };

    private readonly string _path;
    private readonly HashSet<string> _done;

    private CheckpointStore(string path, HashSet<string> done)
    {
        _path = path;
        _done = done;
    }

    /// <summary>Loads an existing checkpoint file, or starts empty if it does not exist.</summary>
    public static async Task<CheckpointStore> LoadAsync(string path, CancellationToken ct = default)
    {
        var done = new HashSet<string>(StringComparer.Ordinal);
        if (File.Exists(path))
        {
            await using var fs = File.OpenRead(path);
            var state = await JsonSerializer.DeserializeAsync<State>(fs, cancellationToken: ct);
            if (state?.Done is not null)
                foreach (var key in state.Done) done.Add(key);
        }
        return new CheckpointStore(path, done);
    }

    /// <summary>True if the given source key was already completed in a previous run.</summary>
    public bool IsDone(string key) => _done.Contains(key);

    /// <summary>Records a source key as completed and persists the checkpoint atomically.</summary>
    public async Task MarkDoneAsync(string key, CancellationToken ct = default)
    {
        if (!_done.Add(key))
            return;

        var state = new State { Done = [.. _done] };
        var tmp = _path + ".tmp";

        var dir = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        await using (var fs = File.Create(tmp))
            await JsonSerializer.SerializeAsync(fs, state, WriteOptions, ct);

        // Atomic replace so a crash never leaves a half-written checkpoint.
        File.Move(tmp, _path, overwrite: true);
    }
}
