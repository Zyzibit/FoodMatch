using System.Text.Json;

namespace inzynierka.IO.Parsing;

/// <summary>
/// Parser JSONL oparty o <see cref="JsonSerializer"/> działający wprost na
/// <see cref="ReadOnlySpan{T}"/> bajtów UTF-8 (pod spodem <c>Utf8JsonReader</c>).
///
/// W porównaniu z klasycznym <c>Deserialize&lt;T&gt;(string)</c> omija:
/// (1) alokację <see cref="string"/> na każdą linię oraz
/// (2) transkodowanie UTF-16 → UTF-8 wykonywane wewnętrznie przy wariancie stringowym.
/// </summary>
public sealed class JsonRecordParser<T> : IRecordParser<T>
{
    private readonly JsonSerializerOptions _options;

    public JsonRecordParser(JsonSerializerOptions? options = null)
    {
        _options = options ?? DefaultOptions;
    }

    public static JsonSerializerOptions DefaultOptions { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    public bool TryParse(ReadOnlySpan<byte> utf8Line, out T value)
    {
        value = JsonSerializer.Deserialize<T>(utf8Line, _options)!;
        return value is not null;
    }
}
