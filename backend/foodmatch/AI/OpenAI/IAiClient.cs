using System.Text.Json;

namespace foodmatch.AI.OpenAI;

public interface IAiClient
{
    Task<JsonElement?> SendPromptForJsonAsync(string systemMessage, string userMessage);
}
