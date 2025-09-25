using System.Text.Json;
using inzynierka.AI.OpenAI.Model;

namespace inzynierka.AI.OpenAI;

public interface IOpenAIClient {
    Task<JsonElement?> SendPromptForJsonasync(List<OpenAIMessage> messages);
}