using System.Text;
using System.Text.Json;
using inzynierka.AI.OpenAI.Config;

namespace inzynierka.AI.OpenAI.Services;

public class PromptConfigService : IPromptConfigService
{
    private readonly ILogger<PromptConfigService> _logger;

    public PromptConfigService(ILogger<PromptConfigService> logger)
    {
        _logger = logger;
    }

    public async Task<PromptConfig> LoadConfigAsync(string configPath)
    {
        try
        {
            var jsonContent = await File.ReadAllTextAsync(configPath);
            var config = JsonSerializer.Deserialize<PromptConfig>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (config == null)
            {
                throw new InvalidOperationException($"Failed to deserialize config from {configPath}");
            }

            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading prompt config from {ConfigPath}", configPath);
            throw;
        }
    }

    public string RenderPrompt(PromptConfig config, Dictionary<string, object?> data)
    {
        var builder = new StringBuilder();

        foreach (var section in config.Sections)
        {
            RenderSection(builder, section, data);
        }

        return builder.ToString();
    }

    private void RenderSection(StringBuilder builder, PromptSection section, Dictionary<string, object?> data, int indentLevel = 0)
    {
        var renderers = new Action[]
        {
            () => AppendIfNotEmpty(builder, section.Title),
            () => AppendIfNotEmpty(builder, ReplacePlaceholders(section.Content ?? "", data)),
            () => AppendIfNotEmpty(builder, ResolvePlaceholder(section.Placeholder ?? "", data)),
            () => RenderItems(builder, section.Items),
            () => RenderDynamicFields(builder, section.DynamicFields, data),
            () => RenderConditionalFields(builder, section.ConditionalFields, data),
            () => RenderJsonSchema(builder, section.JsonSchema),
            () => RenderSubsections(builder, section.Subsections, data, indentLevel),
            () => AppendIfNotEmpty(builder, section.Footer)
        };

        foreach (var render in renderers)
        {
            render();
        }

        builder.AppendLine();
    }

    private void AppendIfNotEmpty(StringBuilder builder, string? text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            builder.AppendLine(text);
        }
    }

    private void RenderItems(StringBuilder builder, List<string>? items)
    {
        items?.ForEach(item => builder.AppendLine(item));
    }

    private void RenderDynamicFields(StringBuilder builder, List<DynamicField>? fields, Dictionary<string, object?> data)
    {
        fields?.ForEach(field =>
        {
            var value = GetFieldValue(data, field.Field);
            if (value != null)
            {
                builder.AppendLine(field.Template.Replace("{value}", value.ToString()));
            }
        });
    }

    private void RenderConditionalFields(StringBuilder builder, List<ConditionalField>? fields, Dictionary<string, object?> data)
    {
        fields?.ForEach(field => RenderConditionalField(builder, field, data));
    }

    private void RenderConditionalField(StringBuilder builder, ConditionalField field, Dictionary<string, object?> data)
    {
        var value = GetFieldValue(data, field.Field);

        var text = value switch
        {
            bool boolValue when boolValue => field.Text,
            not null when !string.IsNullOrEmpty(value.ToString()) => 
                !string.IsNullOrEmpty(field.Template) 
                    ? field.Template.Replace("{value}", value.ToString()) 
                    : field.Text,
            _ => null
        };

        AppendIfNotEmpty(builder, text);
    }

    private void RenderJsonSchema(StringBuilder builder, JsonElement? jsonSchema)
    {
        if (!jsonSchema.HasValue) return;

        var schemaJson = JsonSerializer.Serialize(jsonSchema.Value, new JsonSerializerOptions { WriteIndented = true });
        builder.AppendLine("```json");
        builder.AppendLine(schemaJson);
        builder.AppendLine("```");
    }

    private void RenderSubsections(StringBuilder builder, List<PromptSection>? subsections, Dictionary<string, object?> data, int indentLevel)
    {
        subsections?.ForEach(subsection =>
        {
            builder.AppendLine();
            RenderSection(builder, subsection, data, indentLevel + 1);
        });
    }

    private string ReplacePlaceholders(string text, Dictionary<string, object?> data)
    {
        var result = text;
        
        foreach (var kvp in data)
        {
            var placeholder = $"{{{kvp.Key}}}";
            var value = kvp.Value?.ToString() ?? string.Empty;
            result = result.Replace(placeholder, value);
        }
        
        return result;
    }

    private string? ResolvePlaceholder(string placeholder, Dictionary<string, object?> data)
    {
        var key = placeholder.Trim('{', '}');
        
        if (data.TryGetValue(key, out var value))
        {
            return value?.ToString();
        }

        return null;
    }

    private object? GetFieldValue(Dictionary<string, object?> data, string fieldPath)
    {
        var parts = fieldPath.Split('.');
        object? current = data;

        foreach (var part in parts)
        {
            if (current is Dictionary<string, object?> dict)
            {
                if (!dict.TryGetValue(part, out current))
                {
                    return null;
                }
            }
            else if (current != null)
            {
                var property = current.GetType().GetProperty(part, 
                    System.Reflection.BindingFlags.Public | 
                    System.Reflection.BindingFlags.Instance | 
                    System.Reflection.BindingFlags.IgnoreCase);
                
                if (property != null)
                {
                    current = property.GetValue(current);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        return current;
    }
}

