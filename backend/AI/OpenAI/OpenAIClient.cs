using System.Text;
using System.Text.Json;
using inzynierka.AI.OpenAI.Model;

namespace inzynierka.AI.OpenAI;

public class OpenAIClient : IOpenAIClient {
    private HttpClient _httpClient;
    private IConfiguration _configuration;

    public OpenAIClient(HttpClient httpClient, IConfiguration configuration) {
        _httpClient = httpClient;
        _configuration = configuration;
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_configuration["AI:ApiKey"]}");
    }

    public async Task<JsonElement?> SendPromptForJsonasync(List <OpenAIMessage> messages) {
        OpenAIChatRequest request = new ();
        
        var modelFromConfig = _configuration["AI:Model"];
        request.Model = !string.IsNullOrWhiteSpace(modelFromConfig) ? modelFromConfig : request.Model;
        request.Messages = messages;
        request.Temperature = float.TryParse(_configuration["AI:Temperature"], out var temp) 
            ? temp 
            : request.Temperature;
        
        
        var requestJsonSerialized = JsonSerializer.Serialize(request);
        var content = new StringContent(requestJsonSerialized, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_configuration["AI:ApiLink"], content);
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseString);
        var jsonText = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message").GetProperty("content")
            .GetString();
        try {
            var resultDoc = JsonDocument.Parse(jsonText);
            return resultDoc.RootElement.Clone();
        }
        catch {
            return null;
        }
    }
}