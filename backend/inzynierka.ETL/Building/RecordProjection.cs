namespace inzynierka.ETL.Building;

/// <summary>
/// Result of projecting a single line: whether a record was produced and, if so, its value.
/// "Not produced" means the record was dropped (parse returned false or a <c>Where</c> predicate
/// rejected it) — this is not an error.
///
/// Use <see cref="Keep"/> to carry a value and <see cref="Drop"/> to signal a dropped record.
/// <see cref="Drop"/> is the struct's default, so the dropped case needs no value and no
/// null-forgiving operator. <see cref="Value"/> must only be read when <see cref="Produced"/>.
/// </summary>
internal readonly record struct ProjectionOutcome<T>
{
    public bool Produced { get; }
    public T Value { get; }

    private ProjectionOutcome(bool produced, T value)
    {
        Produced = produced;
        Value = value;
    }

    /// <summary>A produced record carrying <paramref name="value"/>.</summary>
    public static ProjectionOutcome<T> Keep(T value) => new(true, value);

    /// <summary>A dropped record (no value).</summary>
    public static ProjectionOutcome<T> Drop => default;
}

/// <summary>
/// Projection of a single UTF-8 line into an output record. Combines parsing, filtering
/// (<c>Where</c>/<c>WhereAsync</c>) and mapping (<c>Select</c>/<c>SelectAsync</c>) into one
/// operation executed on a worker. The signature is asynchronous to allow async enrichment
/// (e.g. a lookup) in the transform stage, but the common synchronous path completes without
/// allocating a task (the <see cref="ValueTask{TResult}"/> wraps a value).
/// </summary>
internal delegate ValueTask<ProjectionOutcome<TOut>> RecordProjection<TOut>(ReadOnlyMemory<byte> utf8Line);
