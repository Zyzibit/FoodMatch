using System.Text;
using System.Text.Json;
using inzynierka.AI.OpenAI.Model;

namespace inzynierka.AI.OpenAI;

public class OpenAIClient : IOpenAIClient {
    private HttpClient _httpClient;
    private IConfiguration _configuration;
    private readonly ILogger<OpenAIClient> _logger;

    public OpenAIClient(HttpClient httpClient, IConfiguration configuration, ILogger<OpenAIClient> logger) {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_configuration["AI:ApiKey"]}");
    }

    public async Task<JsonElement?> SendPromptForJsonasync(List<OpenAIMessage> messages) {
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
        
        // Handle 429 (Too Many Requests) error
        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("OpenAI API Rate Limit exceeded (429). Response: {ErrorContent}", errorContent);
            
            if (response.Headers.TryGetValues("x-ratelimit-remaining-requests", out var remaining))
            {
                _logger.LogWarning("Remaining requests: {Remaining}", string.Join(", ", remaining));
            }
            
            if (response.Headers.TryGetValues("x-ratelimit-reset-requests", out var reset))
            {
                _logger.LogWarning("Rate limit resets at: {Reset}", string.Join(", ", reset));
            }
            
            throw new HttpRequestException(
                "OpenAI API rate limit exceeded. Please wait a moment and try again. " +
                "Check your OpenAI account for quota limits: https://platform.openai.com/account/limits",
                null,
                System.Net.HttpStatusCode.TooManyRequests
            );
        }
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("OpenAI API request failed with status {StatusCode}. Response: {ErrorContent}", 
                response.StatusCode, errorContent);
            response.EnsureSuccessStatusCode();
        }
        
        var responseString = await response.Content.ReadAsStringAsync();
        
        _logger.LogDebug("OpenAI API raw response: {Response}", responseString);
        
        using var doc = JsonDocument.Parse(responseString);
        var jsonText = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message").GetProperty("content")
            .GetString();
        
        if (string.IsNullOrWhiteSpace(jsonText))
        {
            _logger.LogError("OpenAI returned empty content in response");
            return null;
        }
        
        _logger.LogInformation("OpenAI content before cleanup: {Content}", jsonText);
        
        // Clean up markdown code blocks if present
        var cleanedJson = jsonText.Trim();
        if (cleanedJson.StartsWith("```json"))
        {
            cleanedJson = cleanedJson.Substring(7); // Remove ```json
            var endIndex = cleanedJson.LastIndexOf("```");
            if (endIndex > 0)
            {
                cleanedJson = cleanedJson.Substring(0, endIndex);
            }
        }
        else if (cleanedJson.StartsWith("```"))
        {
            cleanedJson = cleanedJson.Substring(3); // Remove ```
            var endIndex = cleanedJson.LastIndexOf("```");
            if (endIndex > 0)
            {
                cleanedJson = cleanedJson.Substring(0, endIndex);
            }
        }
        
        cleanedJson = cleanedJson.Trim();
        
        // Remove trailing commas before closing braces/brackets (common AI mistake)
        cleanedJson = System.Text.RegularExpressions.Regex.Replace(cleanedJson, @",(\s*[}\]])", "$1");
        
        _logger.LogInformation("OpenAI content after cleanup: {Content}", cleanedJson);
        
        try 
        {
            var resultDoc = JsonDocument.Parse(cleanedJson);
            return resultDoc.RootElement.Clone();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse OpenAI response as JSON. Content: {Content}", cleanedJson);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error parsing OpenAI response");
            return null;
        }
    }
}