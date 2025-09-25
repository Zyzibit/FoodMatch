using System.Text.Json.Serialization;

namespace inzynierka.AI.OpenAI.Model;

public class OpenAIMessage {
    [JsonPropertyName("role")]
    public string Role { get; set; }
    [JsonPropertyName("content")]
    public string Content { get; set; }
    public OpenAIMessage(string role, string content) {
        Role = role;
        Content = content;
    }

}