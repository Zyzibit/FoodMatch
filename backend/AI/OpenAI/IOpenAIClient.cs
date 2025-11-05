using System.Text.Json;
using inzynierka.AI.OpenAI.Model;

namespace inzynierka.AI.OpenAI;

public interface IOpenAIClient {
    Task<JsonElement?> SendPromptForJsonAsync(List<OpenAIMessage> messages);
}