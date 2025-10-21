using System.Text.Json;
using inzynierka.AI.Contracts.Models;
using inzynierka.AI.OpenAI.Model;

namespace inzynierka.AI.OpenAI;

public interface IRecipeGeneratorService
{
    Task<GenerateRecipeResult> GenerateRecipeAsync(GenerateRecipeRequest request);
}

public class RecipeGeneratorService : IRecipeGeneratorService
{
    private readonly IOpenAIClient _openAIClient;
    private readonly ILogger<RecipeGeneratorService> _logger;

    public RecipeGeneratorService(IOpenAIClient openAiClient, ILogger<RecipeGeneratorService> logger)
    {
        _openAIClient = openAiClient;
        _logger = logger;
    }

    public async Task<GenerateRecipeResult> GenerateRecipeAsync(GenerateRecipeRequest request)
    {
        try
        {
            var prompt = BuildRecipePrompt(request);
            var messages = new List<OpenAIMessage>
            {
                new OpenAIMessage(
                    "system",
                    "Jesteś ekspertem kulinarnym, który pomaga użytkownikom tworzyć przepisy kulinarne. " +
                    "Generujesz przepisy w formacie JSON zgodnie z podanym schematem. " +
                    "Zwracaj TYLKO poprawny JSON bez dodatkowych komentarzy."
                ),
                new OpenAIMessage("user", prompt)
            };

            var result = await _openAIClient.SendPromptForJsonasync(messages);
            
            if (result == null)
            {
                return new GenerateRecipeResult
                {
                    Success = false,
                    ErrorMessage = "Failed to parse AI response"
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
                              "Proszę spróbować ponownie za chwilę. " +
                              "Jeśli problem się powtarza, sprawdź limity na koncie OpenAI: https://platform.openai.com/account/limits"
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recipe with AI");
            return new GenerateRecipeResult
            {
                Success = false,
                ErrorMessage = $"Błąd podczas generowania przepisu: {ex.Message}"
            };
        }
    }

    private string BuildRecipePrompt(GenerateRecipeRequest request)
    {
        var promptBuilder = new System.Text.StringBuilder();
        promptBuilder.AppendLine("Wygeneruj przepis kulinarny na podstawie następujących informacji:");
        promptBuilder.AppendLine();
        
        if (request.AvailableIngredients.Any())
        {
            promptBuilder.AppendLine("Dostępne składniki:");
            foreach (var ingredient in request.AvailableIngredients)
            {
                promptBuilder.AppendLine($"- {ingredient}");
            }
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("WAŻNE INSTRUKCJE:");
            promptBuilder.AppendLine("- NIE MUSISZ używać wszystkich podanych składników");
            promptBuilder.AppendLine("- Użyj tylko tych składników, które pasują do siebie i mają sens w przepisie");
            promptBuilder.AppendLine("- Jeśli jakieś składniki nie pasują (np. napoje, gotowe przekąski, produkty z różnych kategorii), po prostu je pomiń");
            promptBuilder.AppendLine("- Jeśli składniki są bardzo różnorodne, wybierz te które najlepiej współgrają i stwórz z nich spójne danie");
            promptBuilder.AppendLine("- Możesz zasugerować dodanie podstawowych składników (sól, pieprz, olej, masło) jeśli są potrzebne");
            promptBuilder.AppendLine();
        }

        if (request.Preferences != null)
        {
            promptBuilder.AppendLine("Preferencje żywieniowe:");
            if (request.Preferences.IsVegetarian) promptBuilder.AppendLine("- Wegetariański");
            if (request.Preferences.IsVegan) promptBuilder.AppendLine("- Wegański");
            if (request.Preferences.IsGlutenFree) promptBuilder.AppendLine("- Bezglutenowy");
            if (request.Preferences.IsLactoseFree) promptBuilder.AppendLine("- Bez laktozy");
            if (request.Preferences.Allergies.Any())
            {
                promptBuilder.AppendLine($"- Alergeny do uniknięcia: {string.Join(", ", request.Preferences.Allergies)}");
            }
            if (request.Preferences.DislikedIngredients.Any())
            {
                promptBuilder.AppendLine($"- Składniki do uniknięcia: {string.Join(", ", request.Preferences.DislikedIngredients)}");
            }
            if (request.Preferences.MaxCalories.HasValue)
            {
                promptBuilder.AppendLine($"- Maksymalna liczba kalorii: {request.Preferences.MaxCalories}");
            }
            promptBuilder.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(request.CuisineType))
        {
            promptBuilder.AppendLine($"Typ kuchni: {request.CuisineType}");
        }

        if (request.DesiredServings.HasValue)
        {
            promptBuilder.AppendLine($"Liczba porcji: {request.DesiredServings}");
        }

        if (request.MaxPreparationTimeMinutes.HasValue)
        {
            promptBuilder.AppendLine($"Maksymalny czas przygotowania: {request.MaxPreparationTimeMinutes} minut");
        }

        if (!string.IsNullOrWhiteSpace(request.AdditionalInstructions))
        {
            promptBuilder.AppendLine($"Dodatkowe wskazówki: {request.AdditionalInstructions}");
        }

        promptBuilder.AppendLine();
        promptBuilder.AppendLine("Zwróć przepis w następującym formacie JSON:");
        promptBuilder.AppendLine(@"{
  ""title"": ""Nazwa przepisu"",
  ""description"": ""Krótki opis przepisu"",
  ""ingredients"": [
    {
      ""name"": ""Nazwa składnika"",
      ""quantity"": ""ilosc składnika jako liczba (np. 100, 0.5)"",
      ""unit"": ""jednostka miary""
    }
  ],
  ""instructions"": ""Krok po kroku instrukcje przygotowania, oddzielone nowymi liniami"",
  ""servings"": ilosc porcji jako liczba calkowitas,
  ""preparationTimeMinutes"": ""fill in total preparation time in minutes"",
  ""estimatedCalories"": ""estimated calories based on ingredients"",
  ""estimatedProtein"": ""estimated protein in grams"",
  ""estimatedCarbohydrates"": ""estimated carbohydrates in grams"",
  ""estimatedFats"": ""estimated fats in grams""
}");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("DOZWOLONE JEDNOSTKI dla pola 'unit':");
        promptBuilder.AppendLine("- gram (dla suchych składników, np. mąka, cukier)");
        promptBuilder.AppendLine("- kilogram (dla dużych ilości suchych składników)");
        promptBuilder.AppendLine("- mililitr (dla płynów, np. mleko, woda)");
        promptBuilder.AppendLine("- litr (dla dużych ilości płynów)");
        promptBuilder.AppendLine("- sztuka (dla produktów liczonych w sztukach, np. jajka, pomidory)");
        promptBuilder.AppendLine("- łyżka (dla przypraw i małych ilości)");
        promptBuilder.AppendLine("- łyżeczka (dla bardzo małych ilości, przypraw)");
        promptBuilder.AppendLine("- szklanka (dla sypkich produktów i płynów)");
        promptBuilder.AppendLine("- opakowanie (dla produktów pakowanych)");
        promptBuilder.AppendLine("- garść (dla produktów sypkich jak orzechy, rodzynki)");
        promptBuilder.AppendLine("- plasterek (dla serów, wędlin, chleba)");
        promptBuilder.AppendLine("- kostka (dla masła, bulionów)");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("WAŻNE: Używaj TYLKO jednostek z powyższej listy. Pisz je małymi literami bez polskich znaków diakrytycznych tam gdzie to możliwe.");

        return promptBuilder.ToString();
    }

    private GeneratedRecipe ParseRecipeFromJson(JsonElement jsonElement)
    {
        var recipe = new GeneratedRecipe
        {
            Title = jsonElement.GetProperty("title").GetString() ?? "",
            Description = jsonElement.GetProperty("description").GetString() ?? "",
            Instructions = jsonElement.GetProperty("instructions").GetString() ?? "",
            EstimatedFats = jsonElement.GetProperty("estimatedFats").GetInt32(),
            EstimatedCalories = jsonElement.GetProperty("estimatedCalories").GetInt32(),
            EstimatedProtein = jsonElement.GetProperty("estimatedProtein").GetInt32(),
            EstimatedCarbohydrates = jsonElement.GetProperty("estimatedCarbohydrates").GetInt32(),
            Servings = jsonElement.GetProperty("servings").GetInt32(),
            PreparationTimeMinutes = jsonElement.GetProperty("preparationTimeMinutes").GetInt32()
        };

        var ingredientsArray = jsonElement.GetProperty("ingredients");
        foreach (var ingredientElement in ingredientsArray.EnumerateArray())
        {
            recipe.Ingredients.Add(new GeneratedRecipeIngredient
            {
                Name = ingredientElement.GetProperty("name").GetString() ?? "",
                Quantity = ingredientElement.GetProperty("quantity").GetDecimal(),
                Unit = ingredientElement.GetProperty("unit").GetString() ?? ""
            });
        }

        return recipe;
    }
}
