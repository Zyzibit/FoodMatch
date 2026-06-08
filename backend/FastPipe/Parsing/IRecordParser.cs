using System.Diagnostics.CodeAnalysis;

namespace FastPipe.Parsing;

/// <summary>
/// Parses a single UTF-8 line into a record of type <typeparamref name="T"/>.
/// Constrained to reference types: a dropped record is represented by <c>null</c>, which has no
/// meaning for a value type.
/// </summary>
public interface IRecordParser<T> where T : class
{
    /// <summary>
    /// Parses a line, throwing on malformed input.
    /// <list type="bullet">
    ///   <item><description>returns a value: a record was parsed.</description></item>
    ///   <item><description>returns <c>null</c>: the line should be dropped (empty/incomplete or a JSON <c>null</c>) — not an error.</description></item>
    ///   <item><description><b>throws</b>: a parse error, routed through the pipeline's error policy.</description></item>
    /// </list>
    /// This is the variant the pipeline uses, so malformed lines surface as exceptions rather than
    /// being silently dropped.
    /// </summary>
    T? Parse(ReadOnlySpan<byte> utf8Line);

    /// <summary>
    /// Non-throwing variant following the standard <c>Try*</c> idiom: returns <c>false</c> for both
    /// a dropped record and malformed input, and never throws on bad data. Prefer this for standalone
    /// use where you don't have an error policy to route exceptions through.
    /// </summary>
    bool TryParse(ReadOnlySpan<byte> utf8Line, [MaybeNullWhen(false)] out T value);
}
