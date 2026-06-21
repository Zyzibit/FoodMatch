namespace foodmatch.ETL.Resilience;

/// <summary>
/// What to do when parsing a single record throws.
/// </summary>
public enum ErrorPolicy
{
    /// <summary>Skip the broken record and count it in the report. Default for bulk imports.</summary>
    Skip = 0,

    /// <summary>Abort the whole pipeline on the first error encountered.</summary>
    Throw = 1,

    /// <summary>
    /// Route the broken record's raw bytes to a configured <see cref="IDeadLetterSink"/>, count it
    /// as failed, and continue. Requires a dead-letter sink (see <c>OnErrorDeadLetter</c>).
    /// </summary>
    DeadLetter = 2
}
