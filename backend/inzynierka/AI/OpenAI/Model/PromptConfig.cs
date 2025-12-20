using System.Text.Json;
using System.Text.Json.Serialization;

namespace inzynierka.AI.OpenAI.Config;

public class PromptConfig
{
    [JsonPropertyName("systemMessage")]
    public string SystemMessage { get; set; } = string.Empty;

    [JsonPropertyName("sections")]
    public List<PromptSection> Sections { get; set; } = new();
}

public class PromptSection
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("placeholder")]
    public string? Placeholder { get; set; }

    [JsonPropertyName("footer")]
    public string? Footer { get; set; }

    [JsonPropertyName("items")]
    public List<string>? Items { get; set; }

    [JsonPropertyName("subsections")]
    public List<PromptSection>? Subsections { get; set; }

    [JsonPropertyName("dynamicFields")]
    public List<DynamicField>? DynamicFields { get; set; }

    [JsonPropertyName("conditionalFields")]
    public List<ConditionalField>? ConditionalFields { get; set; }

    [JsonPropertyName("jsonSchema")]
    public JsonElement? JsonSchema { get; set; }
}

public class DynamicField
{
    [JsonPropertyName("field")]
    public string Field { get; set; } = string.Empty;

    [JsonPropertyName("template")]
    public string Template { get; set; } = string.Empty;
}

public class ConditionalField
{
    [JsonPropertyName("field")]
    public string Field { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("template")]
    public string? Template { get; set; }
}

