using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace inzynierka.IO.Parsing;

/// <summary>
/// Parser JSONL oparty o <see cref="JsonSerializer"/> działający wprost na
/// <see cref="ReadOnlySpan{T}"/> bajtów UTF-8 (pod spodem <c>Utf8JsonReader</c>).
///
/// W porównaniu z klasycznym <c>Deserialize&lt;T&gt;(string)</c> omija:
/// (1) alokację <see cref="string"/> na każdą linię oraz
/// (2) transkodowanie UTF-16 → UTF-8 wykonywane wewnętrznie przy wariancie stringowym.
///
/// Dla maksymalnej wydajności przekaż <see cref="JsonTypeInfo{T}"/> z kontekstu source-gen
/// (<c>JsonSerializerContext</c>) — wtedy parsowanie nie używa refleksji w runtime.
/// </summary>
public sealed class JsonRecordParser<T> : IRecordParser<T>
{
    private readonly JsonSerializerOptions? _options;
    private readonly JsonTypeInfo<T>? _typeInfo;

    /// <summary>Tryb reflection-based (wygodny, wolniejszy).</summary>
    public JsonRecordParser(JsonSerializerOptions? options = null) => _options = options ?? DefaultOptions;

    /// <summary>Tryb source-gen (bez refleksji) — przekaż metadane z <c>JsonSerializerContext</c>.</summary>
    public JsonRecordParser(JsonTypeInfo<T> typeInfo) => _typeInfo = typeInfo;

    public static JsonSerializerOptions DefaultOptions { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    public bool TryParse(ReadOnlySpan<byte> utf8Line, out T value)
    {
        value = _typeInfo is not null
            ? JsonSerializer.Deserialize(utf8Line, _typeInfo)!
            : JsonSerializer.Deserialize<T>(utf8Line, _options)!;
        return value is not null;
    }
}
