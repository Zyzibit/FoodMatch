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

            var response = await _chatClient.CompleteChatAsync(messages, chatOptions);
        
            var content = response.Value.Content;
        
            var jsonText = content[0].Text;
            
            try
            {
                using var resultDoc = JsonDocument.Parse(jsonText);
                return resultDoc.RootElement.Clone();
            }
            catch (JsonException ex)
            {
                throw new FormatException("OpenAI response is not valid JSON.", ex);
            }
        }
        catch (ClientResultException ex) when (ex.Status == 429)
        {
            throw new HttpRequestException(
                "OpenAI API rate limit exceeded. Please wait a moment and try again. " +
                "Check your OpenAI account for quota limits: https://platform.openai.com/account/limits",
                ex,
                System.Net.HttpStatusCode.TooManyRequests
            );
        }
        catch (ClientResultException ex)
        {
            throw new HttpRequestException(
                $"OpenAI API request failed: {ex.Message}",
                ex
            );
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred while communicating with the OpenAI API.", ex);
        }
    }
}