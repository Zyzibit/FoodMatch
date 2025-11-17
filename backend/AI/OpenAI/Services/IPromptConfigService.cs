using inzynierka.AI.OpenAI.Config;

namespace inzynierka.AI.OpenAI.Services;

public interface IPromptConfigService
{
    Task<PromptConfig> LoadConfigAsync(string configPath);
    string RenderPrompt(PromptConfig config, Dictionary<string, object?> data);
}