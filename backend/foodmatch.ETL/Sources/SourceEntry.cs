namespace foodmatch.ETL.Sources;

/// <summary>
/// A data source paired with an optional stable key. The key (an absolute file path for file
/// sources) identifies the source for checkpointing; <c>null</c> means the source is not
/// checkpointable (e.g. a raw stream or pipe reader).
/// </summary>
internal readonly record struct SourceEntry(string? Key, IDataSource Source);
