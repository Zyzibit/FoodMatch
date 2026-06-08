using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace FastPipe.Parsing;

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
///
/// <para><b>Intended for reference types.</b> A JSON <c>null</c> line is reported as a drop
/// (<c>TryParse</c> returns <c>false</c>). For a value-type <typeparamref name="T"/> there is no
/// null to detect, so this drop signal does not apply.</para>
/// </summary>
public sealed class JsonRecordParser<T> : IRecordParser<T>
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

    /// <summary>
    /// Parses one UTF-8 line. Returns <c>true</c> with the value on success; <c>false</c> when the
    /// line should be dropped (empty span, or a JSON <c>null</c> literal). Malformed JSON throws a
    /// <see cref="JsonException"/> — the caller (pipeline) maps that to its error policy.
    /// </summary>
    public bool TryParse(ReadOnlySpan<byte> utf8Line, [MaybeNullWhen(false)] out T value)
    {
        // Empty input is a drop, not a parse error — Deserialize would otherwise throw
        // ("no JSON tokens"). Honours the IRecordParser contract for empty/incomplete lines.
        // O(1): the pipeline already filters blank lines, so we deliberately don't scan for
        // all-whitespace here; such input is treated as malformed and throws.
        if (utf8Line.IsEmpty)
        {
            value = default;
            return false;
        }

        value = _typeInfo is not null
            ? JsonSerializer.Deserialize(utf8Line, _typeInfo)
            : JsonSerializer.Deserialize<T>(utf8Line, _options);
        return value is not null;
    }
}
