using System.Text.Json;
using inzynierka.AI.OpenAI;
using inzynierka.AI.OpenAI.Model;
using inzynierka.AI.OpenAI.Services;
using inzynierka.Receipts.Model.Recipe;
using inzynierka.Receipts.Requests;
using inzynierka.Receipts.Responses;

namespace inzynierka.Receipts.Services;

public class RecipeGeneratorService : IRecipeGeneratorService
{
    private readonly IOpenAIClient _openAiClient;
    private readonly ILogger<RecipeGeneratorService> _logger;
    private readonly IPromptConfigService _promptConfigService;
    private readonly IUnitService _unitService;
    private readonly string _configPath;

    public RecipeGeneratorService(
        IOpenAIClient openAiClient, 
        ILogger<RecipeGeneratorService> logger,
        IPromptConfigService promptConfigService,
        IUnitService unitService,
        IConfiguration configuration)
    {
        _openAiClient = openAiClient;
        _logger = logger;
        _promptConfigService = promptConfigService;
        _unitService = unitService;
        _configPath = configuration["Receipts:PromptConfigPath"] ?? 
                      Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Receipts", "Config", "prompt_config.json");
    }

    public async Task<GenerateRecipeResponse> GenerateRecipeAsync(GenerateRecipeRequest request)
    {
        
        try
        {
            var config = await _promptConfigService.LoadConfigAsync(_configPath);
            
            var data = await PreparePromptDataAsync(request);
            var userPrompt = _promptConfigService.RenderPrompt(config, data);
            
            var messages = new List<OpenAIMessage>
            {
                new OpenAIMessage("system", config.SystemMessage),
                new OpenAIMessage("user", userPrompt)
            };
            
            _logger.LogDebug("Sending prompt to OpenAI with {MessageCount} messages", messages.Count);

            var result = await _openAiClient.SendPromptForJsonAsync(messages);
            
            if (result == null)
            {
                return new GenerateRecipeResponse
                {
                    Success = false,
                    ErrorMessage = "Unable to get a valid response from AI service."
                };
            }
            
            var recipe = ParseRecipeFromJson(result.Value);
            
            return new GenerateRecipeResponse
            {
                Success = true,
                Recipe = recipe
            };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing error in recipe generation");
            return new GenerateRecipeResponse
            {
                Success = false,
                ErrorMessage = "Error while parsing AI response: " + ex.Message
            };
        }
        catch (Exception ex)
        {
            return new GenerateRecipeResponse
            {
                Success = false,
                ErrorMessage = $"Unexpected error while generating recipe: {ex.Message}"
            };
        }
    }

    private GeneratedRecipe ParseRecipeFromJson(JsonElement jsonElement)
    {
        try
        {
            // Walidacja wymaganych pól
            if (!jsonElement.TryGetProperty("title", out _))
            {
                _logger.LogError("Missing required property 'title' in AI response");
                throw new JsonException("AI response missing 'title' property");
            }
            
            if (!jsonElement.TryGetProperty("ingredients", out _))
            {
                _logger.LogError("Missing required property 'ingredients' in AI response");
                throw new JsonException("AI response missing 'ingredients' property");
            }
            
            var recipe = new GeneratedRecipe
            {
                Title = jsonElement.GetProperty("title").GetString() ?? "Invalid title",
                Description = jsonElement.TryGetProperty("description", out var desc) ? desc.GetString() ?? "" : "",
                Instructions = jsonElement.TryGetProperty("instructions", out var instr) ? instr.GetString() ?? "" : "",
                Servings = jsonElement.TryGetProperty("servings", out var serv) ? serv.GetInt32() : 1,
                PreparationTimeMinutes = jsonElement.TryGetProperty("preparationTimeMinutes", out var prep) ? prep.GetInt32() : 0
            };

            var ingredientsArray = jsonElement.GetProperty("ingredients");
            foreach (var ingredientElement in ingredientsArray.EnumerateArray())
            {
                var ingredientName = ingredientElement.GetProperty("name").GetString() ?? "";
                var normalizedQuantity = GetDecimalProperty(ingredientElement, "normalizedQuantityInGrams");
                
                recipe.Ingredients.Add(new GeneratedRecipeIngredient
                {
                    Name = ingredientName,
                    Quantity = GetDecimalProperty(ingredientElement, "quantity"),
                    Unit = ingredientElement.GetProperty("unit").GetString() ?? "",
                    NormalizedQuantityInGrams = normalizedQuantity > 0 ? normalizedQuantity : null,
                    EstimatedCalories = GetDecimalProperty(ingredientElement, "estimatedCalories"),
                    EstimatedProteins = GetDecimalProperty(ingredientElement, "estimatedProteins"),
                    EstimatedCarbohydrates = GetDecimalProperty(ingredientElement, "estimatedCarbohydrates"),
                    EstimatedFats = GetDecimalProperty(ingredientElement, "estimatedFats")
                });
            }

            decimal actualTotalWeight = 0;
            foreach (var i in recipe.Ingredients)
            {
                if (i.NormalizedQuantityInGrams.HasValue) actualTotalWeight += i.NormalizedQuantityInGrams.Value;
            }

            var actualTotalCalories = recipe.Ingredients.Sum(i => i.EstimatedCalories);
            var actualTotalProtein = recipe.Ingredients.Sum(i => i.EstimatedProteins);
            var actualTotalCarbs = recipe.Ingredients.Sum(i => i.EstimatedCarbohydrates);
            var actualTotalFats = recipe.Ingredients.Sum(i => i.EstimatedFats);
            
            var aiTotalWeight = GetIntProperty(jsonElement, "totalWeightGrams");
            var aiTotalCalories = GetDecimalProperty(jsonElement, "estimatedCalories");
            if (aiTotalWeight > 0 && Math.Abs(aiTotalWeight - actualTotalWeight) > 1m)
            {
                _logger.LogWarning(
                    "AI provided totalWeightGrams ({AIWeight}g) differs from actual sum ({ActualWeight}g). Using actual weight.",
                    aiTotalWeight, actualTotalWeight);
            }
            
            if (aiTotalCalories > 0 && Math.Abs(aiTotalCalories - actualTotalCalories) > 10m)
            {
                _logger.LogWarning(
                    "AI provided estimatedCalories ({AICalories} kcal) differs from actual sum ({ActualCalories} kcal). Using actual values.",
                    aiTotalCalories, actualTotalCalories);
            }
            
            recipe.TotalWeightGrams = (int)Math.Round(actualTotalWeight);
            
            var weightFactor = actualTotalWeight > 0 ? 100m / actualTotalWeight : 0m;
            recipe.EstimatedCalories = actualTotalCalories * weightFactor;
            recipe.EstimatedProtein = actualTotalProtein * weightFactor;
            recipe.EstimatedCarbohydrates = actualTotalCarbs * weightFactor;
            recipe.EstimatedFats = actualTotalFats * weightFactor;

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
    
    private async Task<Dictionary<string, object?>> PreparePromptDataAsync(GenerateRecipeRequest request)
    {
        var units = await _unitService.GetAllUnitsAsync();
        var unitNames = string.Join("\n", units.Select(u => u.Name));
        
        var data = new Dictionary<string, object?>
        {
            ["availableIngredients"] = request.AvailableIngredients.Any()
                ? string.Join("\n", request.AvailableIngredients)
                : "",
            ["allowedUnits"] = unitNames,
            ["cuisineType"] = request.CuisineType,
            ["desiredServings"] = request.DesiredServings,
            ["maxPreparationTimeMinutes"] = request.MaxPreparationTimeMinutes,
            ["additionalInstructions"] = request.AdditionalInstructions
        };

        if (request.Preferences != null)
        {
            data["isVegan"] = request.Preferences.IsVegan;
            data["isVegetarian"] = request.Preferences.IsVegetarian;
            data["isGlutenFree"] = request.Preferences.IsGlutenFree;
            data["isLactoseFree"] = request.Preferences.IsLactoseFree;
            data["maxCalories"] = request.Preferences.MaxCalories;
            data["allergies"] = request.Preferences.Allergies.Any() 
                ? string.Join(", ", request.Preferences.Allergies) 
                : "brak";
            data["dislikedIngredients"] = request.Preferences.DislikedIngredients.Any()
                ? string.Join(", ", request.Preferences.DislikedIngredients)
                : "brak";
            
            if (request.Preferences.TargetMealCalories.HasValue)
            {
                data["targetMealCalories"] = request.Preferences.TargetMealCalories.Value;
            }
            
            if (!string.IsNullOrEmpty(request.Preferences.MealType))
            {
                data["mealType"] = request.Preferences.MealType;
            }
            
            if (request.Preferences.DailyCalorieGoal.HasValue)
            {
                data["dailyCalorieGoal"] = request.Preferences.DailyCalorieGoal.Value;
            }
            
            if (request.Preferences.DailyProteinGoal.HasValue)
            {
                data["dailyProteinGoal"] = request.Preferences.DailyProteinGoal.Value;
            }
            
            if (request.Preferences.DailyCarbohydrateGoal.HasValue)
            {
                data["dailyCarbohydrateGoal"] = request.Preferences.DailyCarbohydrateGoal.Value;
            }
            
            if (request.Preferences.DailyFatGoal.HasValue)
            {
                data["dailyFatGoal"] = request.Preferences.DailyFatGoal.Value;
            }
        }
        return data;
    }
}
