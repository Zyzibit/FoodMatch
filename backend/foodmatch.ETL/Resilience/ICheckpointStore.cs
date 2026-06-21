namespace inzynierka.ETL.Resilience;

/// <summary>
/// File-granularity checkpoint for multi-source runs. Records which source keys (e.g. absolute
/// file paths) already completed so a re-run resumes from the first unprocessed source. Implement
/// this to persist progress elsewhere (database, cache, …); <see cref="FileCheckpointStore"/> is
/// the default JSON-file implementation.
/// </summary>
public interface ICheckpointStore
{
    /// <summary>True if the given source key was already completed in a previous run.</summary>
    bool IsDone(string key);

    /// <summary>Records a source key as completed and persists the checkpoint.</summary>
    Task MarkDoneAsync(string key, CancellationToken ct = default);
}
