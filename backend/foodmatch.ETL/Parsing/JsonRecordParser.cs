using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace foodmatch.ETL.Parsing;

/// <summary>
/// JSONL parser based on <see cref="JsonSerializer"/> working directly on a
/// <see cref="ReadOnlySpan{T}"/> of UTF-8 bytes (backed by <c>Utf8JsonReader</c>).
///
/// Compared with the classic <c>Deserialize&lt;T&gt;(string)</c> it avoids:
/// (1) allocating a <see cref="string"/> per line, and
/// (2) the UTF-16 → UTF-8 transcoding done internally by the string overload.
///
/// For maximum throughput pass a <see cref="JsonTypeInfo{T}"/> from a source-gen context
/// (<c>JsonSerializerContext</c>) — parsing then uses no runtime reflection.
///
/// <para><b>Two constructors, two behaviours.</b> The reflection constructor applies
/// <see cref="DefaultOptions"/> (case-insensitive names, trailing commas) unless overridden;
/// the source-gen constructor ignores those and uses whatever <c>JsonSourceGenerationOptions</c>
/// the supplied <see cref="JsonTypeInfo{T}"/> was generated with. The same input may therefore
/// parse differently depending on which constructor was used — configure them to match if you
/// need identical behaviour.</para>
/// </summary>
public sealed class JsonRecordParser<T> : IRecordParser<T> where T : class
{
    private readonly JsonSerializerOptions? _options;
    private readonly JsonTypeInfo<T>? _typeInfo;

    /// <summary>Reflection-based mode (convenient, slower).</summary>
    public JsonRecordParser(JsonSerializerOptions? options = null) => _options = options ?? DefaultOptions;

    /// <summary>Source-gen mode (no reflection) — pass metadata from a <c>JsonSerializerContext</c>.</summary>
    public JsonRecordParser(JsonTypeInfo<T> typeInfo) => _typeInfo = typeInfo;

    public static JsonSerializerOptions DefaultOptions { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    /// <inheritdoc/>
    public T? Parse(ReadOnlySpan<byte> utf8Line)
    {
        // Empty input is a drop, not a parse error — Deserialize would otherwise throw
        // ("no JSON tokens"). The pipeline already filters blank lines, so we deliberately
        // don't scan for all-whitespace here; such input is treated as malformed and throws.
        if (utf8Line.IsEmpty)
            return null;

        return _typeInfo is not null
            ? JsonSerializer.Deserialize(utf8Line, _typeInfo)
            : JsonSerializer.Deserialize<T>(utf8Line, _options);
    }

    /// <inheritdoc/>
    public bool TryParse(ReadOnlySpan<byte> utf8Line, [MaybeNullWhen(false)] out T value)
    {
        try
        {
            if (Parse(utf8Line) is { } parsed)
            {
                value = parsed;
                return true;
            }
        }
        catch (JsonException)
        {
            // Malformed input — fall through to the dropped result below.
        }

        value = null;
        return false;
    }
}
