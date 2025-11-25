using System.Text.Json;

namespace inzynierka.AI.OpenAI;

public interface IAiClient
{
    Task<JsonElement?> SendPromptForJsonAsync(string systemMessage, string userMessage);
}
