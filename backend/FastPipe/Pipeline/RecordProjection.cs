namespace FastPipe.Pipeline;

/// <summary>
/// Result of projecting a single line: whether a record was produced and, if so, its value.
/// <see cref="Produced"/> == <c>false</c> means the record was dropped (parse returned false
/// or a <c>Where</c> predicate rejected it) — this is not an error.
/// </summary>
internal readonly record struct ProjectionOutcome<T>(bool Produced, T Value)
{
    public static ProjectionOutcome<T> Dropped => new(false, default!);
}

/// <summary>
/// Projection of a single UTF-8 line into an output record. Combines parsing, filtering
/// (<c>Where</c>/<c>WhereAsync</c>) and mapping (<c>Select</c>/<c>SelectAsync</c>) into one
/// operation executed on a worker. The signature is asynchronous to allow async enrichment
/// (e.g. a lookup) in the transform stage, but the common synchronous path completes without
/// allocating a task (the <see cref="ValueTask{TResult}"/> wraps a value).
/// </summary>
internal delegate ValueTask<ProjectionOutcome<TOut>> RecordProjection<TOut>(ReadOnlyMemory<byte> utf8Line);
