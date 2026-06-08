namespace FastPipe.Pipeline;

/// <summary>
/// What to do when parsing a single record throws.
/// </summary>
public enum ErrorPolicy
{
    /// <summary>Skip the broken record and count it in the report. Default for bulk imports.</summary>
    Skip = 0,

    /// <summary>Abort the whole pipeline on the first error encountered.</summary>
    Throw = 1
}
