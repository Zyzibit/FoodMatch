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
        if (!string.IsNullOrEmpty(section.Title))
        {
            builder.AppendLine(section.Title);
        }

        if (!string.IsNullOrEmpty(section.Content))
        {
            // Zastąp wszystkie placeholdery w contencie
            var renderedContent = ReplacePlaceholders(section.Content, data);
            builder.AppendLine(renderedContent);
        }

        if (!string.IsNullOrEmpty(section.Placeholder))
        {
            var placeholderValue = ResolvePlaceholder(section.Placeholder, data);
            if (!string.IsNullOrEmpty(placeholderValue))
            {
                builder.AppendLine(placeholderValue);
            }
        }

        if (section.Items != null && section.Items.Any())
        {
            foreach (var item in section.Items)
            {
                builder.AppendLine(item);
            }
        }

        if (section.DynamicFields != null)
        {
            foreach (var field in section.DynamicFields)
            {
                var value = GetFieldValue(data, field.Field);
                if (value != null)
                {
                    var rendered = field.Template.Replace("{value}", value.ToString());
                    builder.AppendLine(rendered);
                }
            }
        }

        if (section.ConditionalFields != null)
        {
            foreach (var field in section.ConditionalFields)
            {
                var value = GetFieldValue(data, field.Field);
                
                if (value is bool boolValue)
                {
                    if (boolValue && !string.IsNullOrEmpty(field.Text))
                    {
                        builder.AppendLine(field.Text);
                    }
                }
                else if (value != null && !string.IsNullOrEmpty(value.ToString()))
                {
                    if (!string.IsNullOrEmpty(field.Template))
                    {
                        var rendered = field.Template.Replace("{value}", value.ToString());
                        builder.AppendLine(rendered);
                    }
                    else if (!string.IsNullOrEmpty(field.Text))
                    {
                        builder.AppendLine(field.Text);
                    }
                }
            }
        }

        if (section.JsonSchema.HasValue)
        {
            var schemaJson = JsonSerializer.Serialize(section.JsonSchema.Value, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            builder.AppendLine("```json");
            builder.AppendLine(schemaJson);
            builder.AppendLine("```");
        }

        if (section.Subsections != null)
        {
            foreach (var subsection in section.Subsections)
            {
                builder.AppendLine();
                RenderSection(builder, subsection, data, indentLevel + 1);
            }
        }

        if (!string.IsNullOrEmpty(section.Footer))
        {
            builder.AppendLine(section.Footer);
        }

        builder.AppendLine();
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

