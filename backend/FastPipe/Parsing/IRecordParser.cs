using System.Diagnostics.CodeAnalysis;

namespace FastPipe.Parsing;

public interface IRecordParser<T>
{
    /// <summary>
    /// Parses one UTF-8 line. Three outcomes:
    /// <list type="bullet">
    ///   <item><description><c>true</c> + <paramref name="value"/>: a record was parsed.</description></item>
    ///   <item><description><c>false</c>: the line should be dropped (e.g. empty/incomplete) — not an error.</description></item>
    ///   <item><description><b>throws</b>: a parse error, handled according to the pipeline's error policy.</description></item>
    /// </list>
    /// Note this is not the usual non-throwing <c>Try*</c> idiom: malformed input throws by design,
    /// so the pipeline can route it through <c>ErrorPolicy</c> rather than silently dropping it.
    /// </summary>
    bool TryParse(ReadOnlySpan<byte> utf8Line, [MaybeNullWhen(false)] out T value);
}
