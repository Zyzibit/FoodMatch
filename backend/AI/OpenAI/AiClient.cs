using System.ClientModel;
using System.Text.Json;
using OpenAI.Chat;

namespace inzynierka.AI.OpenAI;

public class AiClient : IAiClient
{
    private readonly ChatClient _chatClient;
    private readonly ILogger<AiClient> _logger;
    private readonly string _model;
    private readonly float _temperature;

    public AiClient(IConfiguration configuration, ILogger<AiClient> logger)
    {
        _logger = logger;
        
        var apiKey = configuration["AI:ApiKey"] 
            ?? throw new InvalidOperationException("AI:ApiKey configuration is missing");
        
        _model = configuration["AI:Model"] ?? "gpt-4o-mini";
        _temperature = float.TryParse(configuration["AI:Temperature"], out var temp) ? temp : 0.9f;
        
        _chatClient = new ChatClient(_model, new ApiKeyCredential(apiKey));
        
        _logger.LogInformation("OpenAI Client initialized with model: {Model}, temperature: {Temperature}", 
            _model, _temperature);
    }

    public async Task<JsonElement?> SendPromptForJsonAsync(string systemMessage, string userMessage)
    {
        try
        {
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemMessage),
                new UserChatMessage(userMessage)
            };

            var chatOptions = new ChatCompletionOptions
            {
                Temperature = _temperature,
                ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
            };

            _logger.LogDebug("Sending request to OpenAI with model: {Model}", _model);
            
            var response = await _chatClient.CompleteChatAsync(messages, chatOptions);
            
            if (response?.Value == null)
            {
                _logger.LogError("OpenAI returned null response");
                return null;
            }

            var content = response.Value.Content;
            
            if (content == null || content.Count == 0)
            {
                _logger.LogError("OpenAI returned empty content in response");
                return null;
            }

            var jsonText = content[0].Text;
            
            if (string.IsNullOrWhiteSpace(jsonText))
            {
                _logger.LogError("OpenAI returned empty text content");
                return null;
            }

            _logger.LogDebug("OpenAI response received, parsing JSON");

            try
            {
                var resultDoc = JsonDocument.Parse(jsonText);
                return resultDoc.RootElement.Clone();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse OpenAI response as JSON. Content: {Content}", jsonText);
                return null;
            }
        }
        catch (ClientResultException ex) when (ex.Status == 429)
        {
            _logger.LogError(ex, "OpenAI API Rate Limit exceeded (429)");
            
            throw new HttpRequestException(
                "OpenAI API rate limit exceeded. Please wait a moment and try again. " +
                "Check your OpenAI account for quota limits: https://platform.openai.com/account/limits",
                ex,
                System.Net.HttpStatusCode.TooManyRequests
            );
        }
        catch (ClientResultException ex)
        {
            _logger.LogError(ex, "OpenAI API request failed with status {StatusCode}", ex.Status);
            throw new HttpRequestException(
                $"OpenAI API request failed: {ex.Message}",
                ex
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during OpenAI API call");
            throw;
        }
    }
}