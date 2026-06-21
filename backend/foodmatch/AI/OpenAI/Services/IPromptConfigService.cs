using foodmatch.AI.OpenAI.Config;

namespace foodmatch.AI.OpenAI.Services;

public interface IPromptConfigService
{
    Task<PromptConfig> LoadConfigAsync(string configPath);
    string RenderPrompt(PromptConfig config, Dictionary<string, object?> data);
}