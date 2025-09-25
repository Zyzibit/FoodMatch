using System.Text.Json.Serialization;

namespace inzynierka.AI.OpenAI.Model;

public class OpenAIChatRequest {
    [JsonPropertyName("model")] 
    public string Model { get; set; } = "gpt-4o-mini";
    [JsonPropertyName("messages")] 
    public List <OpenAIMessage> Messages { get; set; }

    [JsonPropertyName("temperature")] public float Temperature { get; set; } = 1.0f;

}