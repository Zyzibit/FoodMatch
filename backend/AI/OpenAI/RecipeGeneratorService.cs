using System.Text.Json;
using inzynierka.AI.Contracts.Models;
using inzynierka.AI.OpenAI.Model;
using inzynierka.AI.OpenAI.PromptBuilders;

namespace inzynierka.AI.OpenAI;

public interface IRecipeGeneratorService
{
    Task<GenerateRecipeResult> GenerateRecipeAsync(GenerateRecipeRequest request);
}

public class RecipeGeneratorService : IRecipeGeneratorService
{
    private readonly IOpenAIClient _openAIClient;
    private readonly ILogger<RecipeGeneratorService> _logger;
    private readonly IRecipePromptBuilder _promptBuilder;

    public RecipeGeneratorService(
        IOpenAIClient openAiClient, 
        ILogger<RecipeGeneratorService> logger,
        IRecipePromptBuilder promptBuilder)
    {
        _openAIClient = openAiClient;
        _logger = logger;
        _promptBuilder = promptBuilder;
    }

    public async Task<GenerateRecipeResult> GenerateRecipeAsync(GenerateRecipeRequest request)
    {
        try
        {
            var prompt = await _promptBuilder.BuildPromptAsync(request);
            var messages = new List<OpenAIMessage>
            {
                new OpenAIMessage(
                    "system",
                    "Jesteś ekspertem kulinarnym specjalizującym się w tworzeniu praktycznych, " +
                    "smacznych i zdrowych przepisów. Tworzysz przepisy w formacie JSON zgodnie z instrukcjami. " +
                    "Zwracasz WYŁĄCZNIE poprawny JSON bez dodatkowych komentarzy, wyjaśnień czy formatowania markdown."
                ),
                new OpenAIMessage("user", prompt)
            };

            var result = await _openAIClient.SendPromptForJsonasync(messages);
            
            if (result == null)
            {
                _logger.LogError("Failed to parse AI response - received null");
                return new GenerateRecipeResult
                {
                    Success = false,
                    ErrorMessage = "Nie udało się przetworzyć odpowiedzi AI"
                };
            }

            var recipe = ParseRecipeFromJson(result.Value);
            
            return new GenerateRecipeResult
            {
                Success = true,
                Recipe = recipe
            };
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            _logger.LogError(ex, "OpenAI rate limit exceeded");
            return new GenerateRecipeResult
            {
                Success = false,
                ErrorMessage = "Przekroczono limit zapytań do OpenAI API. " +
                              "Proszę spróbować ponownie za chwilę."
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while generating recipe with AI");
            return new GenerateRecipeResult
            {
                Success = false,
                ErrorMessage = $"Błąd połączenia z OpenAI API: {ex.Message}"
            };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing error in recipe generation");
            return new GenerateRecipeResult
            {
                Success = false,
                ErrorMessage = "Błąd w formacie odpowiedzi AI. Spróbuj ponownie."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error generating recipe with AI");
            return new GenerateRecipeResult
            {
                Success = false,
                ErrorMessage = $"Nieoczekiwany błąd podczas generowania przepisu: {ex.Message}"
            };
        }
    }

    private GeneratedRecipe ParseRecipeFromJson(JsonElement jsonElement)
    {
        try
        {
            var recipe = new GeneratedRecipe
            {
                Title = jsonElement.GetProperty("title").GetString() ?? "Bez tytułu",
                Description = jsonElement.GetProperty("description").GetString() ?? "",
                Instructions = jsonElement.GetProperty("instructions").GetString() ?? "",
                Servings = jsonElement.GetProperty("servings").GetInt32(),
                PreparationTimeMinutes = jsonElement.GetProperty("preparationTimeMinutes").GetInt32(),
                TotalWeightGrams = GetIntProperty(jsonElement, "totalWeightGrams"),
                EstimatedCalories = GetIntProperty(jsonElement, "estimatedCalories"),
                EstimatedProtein = GetIntProperty(jsonElement, "estimatedProtein"),
                EstimatedCarbohydrates = GetIntProperty(jsonElement, "estimatedCarbohydrates"),
                EstimatedFats = GetIntProperty(jsonElement, "estimatedFats")
            };

            var ingredientsArray = jsonElement.GetProperty("ingredients");
            foreach (var ingredientElement in ingredientsArray.EnumerateArray())
            {
                recipe.Ingredients.Add(new GeneratedRecipeIngredient
                {
                    Name = ingredientElement.GetProperty("name").GetString() ?? "",
                    Quantity = GetDecimalProperty(ingredientElement, "quantity"),
                    Unit = ingredientElement.GetProperty("unit").GetString() ?? ""
                });
            }

            return recipe;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing recipe JSON structure");
            throw new JsonException("Błąd parsowania struktury przepisu", ex);
        }
    }

    private int GetIntProperty(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
            return 0;

        return property.ValueKind switch
        {
            JsonValueKind.Number => property.GetInt32(),
            JsonValueKind.String when int.TryParse(property.GetString(), out var value) => value,
            _ => 0
        };
    }

    private decimal GetDecimalProperty(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
            return 0;

        return property.ValueKind switch
        {
            JsonValueKind.Number => property.GetDecimal(),
            JsonValueKind.String when decimal.TryParse(property.GetString(), out var value) => value,
            _ => 0
        };
    }
}
