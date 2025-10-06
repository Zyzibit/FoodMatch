using inzynierka.AI.Contracts;
using inzynierka.AI.Contracts.Models;
using inzynierka.AI.OpenAI;
using inzynierka.AI.OpenAI.Model;
using inzynierka.EventBus;
using inzynierka.AI.EventBus.Events;
using System.Text.Json;

namespace inzynierka.AI.Modules;

/// <summary>
/// Implementacja kontraktu AI - modu³ AI
/// </summary>
public class AIModule : IAIContract
{
    private readonly IOpenAIClient _openAIClient;
    private readonly IEventBus _eventBus;
    private readonly ILogger<AIModule> _logger;

    public AIModule(
        IOpenAIClient openAIClient,
        IEventBus eventBus,
        ILogger<AIModule> logger)
    {
        _openAIClient = openAIClient;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<AITextResult> GenerateResponseAsync(List<AIMessage> messages, AIGenerationOptions? options = null)
    {
        try
        {
            var startTime = DateTime.UtcNow;
            var openAIMessages = messages.Select(m => new OpenAIMessage(m.Role, m.Content)).ToList();

            var jsonResponse = await _openAIClient.SendPromptForJsonasync(openAIMessages);
            
            string response = "";
            if (jsonResponse.HasValue)
            {
                if (jsonResponse.Value.TryGetProperty("content", out JsonElement contentElement))
                {
                    response = contentElement.GetString() ?? "";
                }
                else
                {
                    response = jsonResponse.Value.ToString();
                }
            }

            var processingTime = DateTime.UtcNow - startTime;

            // Publikacja zdarzenia generowania tekstu
            await _eventBus.PublishAsync(new AITextGeneratedEvent
            {
                UserId = "",
                Model = options?.Model ?? "gpt-3.5-turbo",
                TokensUsed = options?.MaxTokens ?? 0,
                ProcessingTime = processingTime
            });

            return new AITextResult
            {
                Success = true,
                Response = response,
                ModelUsed = options?.Model ?? "gpt-3.5-turbo",
                TokensUsed = options?.MaxTokens
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating AI response");
            return new AITextResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<AIJsonResult> GenerateJsonAsync(List<AIMessage> messages, string? schema = null)
    {
        try
        {
            var openAIMessages = messages.Select(m => new OpenAIMessage(m.Role, m.Content)).ToList();

            if (!string.IsNullOrEmpty(schema))
            {
                openAIMessages.Add(new OpenAIMessage("system", $"Return response in JSON format according to this schema: {schema}"));
            }

            var response = await _openAIClient.SendPromptForJsonasync(openAIMessages);

            string jsonString = response?.ToString() ?? "";
            bool isValidJson = response.HasValue;

            // Publikacja zdarzenia generowania JSON
            await _eventBus.PublishAsync(new AIJsonGeneratedEvent
            {
                UserId = "",
                IsValidJson = isValidJson,
                Schema = schema ?? ""
            });

            return new AIJsonResult
            {
                Success = true,
                JsonResponse = jsonString,
                IsValidJson = isValidJson
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating AI JSON response");
            return new AIJsonResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<ProductAnalysisResult> AnalyzeProductAsync(string productId, ProductAnalysisType analysisType)
    {
        try
        {
            var startTime = DateTime.UtcNow;

            // Publikacja zdarzenia rozpoczêcia analizy
            await _eventBus.PublishAsync(new AIAnalysisRequestedEvent
            {
                ProductId = productId,
                AnalysisType = analysisType.ToString()
            });

            var analysisPrompt = analysisType switch
            {
                ProductAnalysisType.Nutritional => "Analyze the nutritional value and health benefits of this product.",
                ProductAnalysisType.Allergens => "Identify potential allergens and allergy-related risks in this product.",
                ProductAnalysisType.Ingredients => "Analyze the ingredients list and their quality.",
                ProductAnalysisType.Environmental => "Assess the environmental impact and sustainability of this product.",
                ProductAnalysisType.Health => "Provide a comprehensive health assessment of this product.",
                ProductAnalysisType.Dietary => "Analyze dietary compatibility (vegan, vegetarian, gluten-free, etc.).",
                _ => "Provide a general analysis of this product."
            };

            var messages = new List<OpenAIMessage>
            {
                new OpenAIMessage("system", $"You are a food expert. {analysisPrompt} Product ID: {productId}"),
                new OpenAIMessage("user", "Provide detailed analysis.")
            };

            var response = await _openAIClient.SendPromptForJsonasync(messages);
            
            string analysisResult = "";
            var tags = new List<string>();
            var metadata = new Dictionary<string, object>();

            if (response.HasValue)
            {
                if (response.Value.TryGetProperty("analysis", out JsonElement analysisElement))
                {
                    analysisResult = analysisElement.GetString() ?? "";
                }
                else
                {
                    analysisResult = response.Value.ToString();
                }

                if (response.Value.TryGetProperty("tags", out JsonElement tagsElement) && tagsElement.ValueKind == JsonValueKind.Array)
                {
                    tags = tagsElement.EnumerateArray().Select(tag => tag.GetString() ?? "").ToList();
                }

                if (response.Value.TryGetProperty("metadata", out JsonElement metadataElement))
                {
                    foreach (var prop in metadataElement.EnumerateObject())
                    {
                        metadata[prop.Name] = prop.Value.ToString();
                    }
                }
            }

            var processingTime = DateTime.UtcNow - startTime;
            const double confidenceScore = 0.85;

            // Publikacja zdarzenia zakoñczenia analizy
            await _eventBus.PublishAsync(new AIAnalysisCompletedEvent
            {
                ProductId = productId,
                AnalysisType = analysisType.ToString(),
                ConfidenceScore = confidenceScore,
                ProcessingTime = processingTime
            });

            return new ProductAnalysisResult
            {
                Success = true,
                Analysis = analysisResult,
                ConfidenceScore = confidenceScore,
                Tags = tags,
                Metadata = metadata
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing product: {ProductId}", productId);
            return new ProductAnalysisResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<RecipeRecommendationResult> GetRecipeRecommendationsAsync(List<string> ingredients, DietaryPreferences? preferences = null)
    {
        try
        {
            var ingredientsText = string.Join(", ", ingredients);
            var preferencesText = "";

            if (preferences != null)
            {
                var prefs = new List<string>();
                if (preferences.IsVegetarian) prefs.Add("vegetarian");
                if (preferences.IsVegan) prefs.Add("vegan");
                if (preferences.IsGlutenFree) prefs.Add("gluten-free");
                if (preferences.IsLactoseFree) prefs.Add("lactose-free");
                
                preferencesText = prefs.Count > 0 ? $" Dietary preferences: {string.Join(", ", prefs)}." : "";
                
                if (preferences.Allergies.Count > 0)
                {
                    preferencesText += $" Avoid allergens: {string.Join(", ", preferences.Allergies)}.";
                }
            }

            var messages = new List<OpenAIMessage>
            {
                new OpenAIMessage("system", "You are a chef AI. Generate recipe recommendations based on available ingredients and dietary preferences."),
                new OpenAIMessage("user", $"Available ingredients: {ingredientsText}.{preferencesText} Provide 3-5 recipe suggestions in JSON format.")
            };

            var response = await _openAIClient.SendPromptForJsonasync(messages);
            
            var recommendations = new List<RecipeRecommendation>();

            if (response.HasValue)
            {
                if (response.Value.TryGetProperty("recipes", out JsonElement recipesElement) && recipesElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var recipeElement in recipesElement.EnumerateArray())
                    {
                        var recipe = new RecipeRecommendation();
                        
                        if (recipeElement.TryGetProperty("name", out var nameElement))
                            recipe.Name = nameElement.GetString() ?? "";
                        
                        if (recipeElement.TryGetProperty("description", out var descElement))
                            recipe.Description = descElement.GetString() ?? "";
                        
                        if (recipeElement.TryGetProperty("prep_time", out var prepElement))
                            recipe.PrepTimeMinutes = prepElement.GetInt32();
                        
                        if (recipeElement.TryGetProperty("match_score", out var scoreElement))
                            recipe.MatchScore = scoreElement.GetDouble();
                        
                        recommendations.Add(recipe);
                    }
                }
            }

            // Publikacja zdarzenia generowania przepisów
            await _eventBus.PublishAsync(new RecipeGeneratedEvent
            {
                Ingredients = ingredients,
                RecipeCount = recommendations.Count
            });

            return new RecipeRecommendationResult
            {
                Success = true,
                Recommendations = recommendations
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recipe recommendations");
            return new RecipeRecommendationResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<NutritionalAnalysisResult> AnalyzeNutritionAsync(string productId)
    {
        try
        {
            var messages = new List<OpenAIMessage>
            {
                new OpenAIMessage("system", "You are a nutritionist AI. Analyze the nutritional value of products."),
                new OpenAIMessage("user", $"Analyze nutrition for product ID: {productId}. Provide detailed nutritional assessment.")
            };

            var response = await _openAIClient.SendPromptForJsonasync(messages);
            
            string analysis = "";
            var recommendations = new List<string>();
            var score = new NutritionScore();

            if (response.HasValue)
            {
                if (response.Value.TryGetProperty("analysis", out JsonElement analysisElement))
                    analysis = analysisElement.GetString() ?? "";
                
                if (response.Value.TryGetProperty("recommendations", out JsonElement recsElement) && recsElement.ValueKind == JsonValueKind.Array)
                {
                    recommendations = recsElement.EnumerateArray().Select(r => r.GetString() ?? "").ToList();
                }
                
                if (response.Value.TryGetProperty("score", out JsonElement scoreElement))
                {
                    if (scoreElement.TryGetProperty("overall", out var overallElement))
                        score.Overall = overallElement.GetDouble();
                }
            }

            // Publikacja zdarzenia analizy ¿ywieniowej
            await _eventBus.PublishAsync(new NutritionalAnalysisPerformedEvent
            {
                ProductId = productId,
                UserId = "",
                OverallScore = score.Overall
            });

            return new NutritionalAnalysisResult
            {
                Success = true,
                Analysis = analysis,
                Score = score,
                Recommendations = recommendations
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing nutrition for product: {ProductId}", productId);
            return new NutritionalAnalysisResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<AllergenDetectionResult> DetectAllergensAsync(List<string> ingredients)
    {
        try
        {
            var ingredientsText = string.Join(", ", ingredients);
            
            var messages = new List<OpenAIMessage>
            {
                new OpenAIMessage("system", "You are an allergen detection expert. Identify potential allergens in ingredient lists."),
                new OpenAIMessage("user", $"Detect allergens in these ingredients: {ingredientsText}. Return results in JSON format.")
            };

            var response = await _openAIClient.SendPromptForJsonasync(messages);
            
            var detectedAllergens = new List<DetectedAllergen>();

            if (response.HasValue)
            {
                if (response.Value.TryGetProperty("allergens", out JsonElement allergensElement) && allergensElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var allergenElement in allergensElement.EnumerateArray())
                    {
                        var allergen = new DetectedAllergen();
                        
                        if (allergenElement.TryGetProperty("name", out var nameElement))
                            allergen.Name = nameElement.GetString() ?? "";
                        
                        if (allergenElement.TryGetProperty("confidence", out var confElement))
                            allergen.Confidence = confElement.GetDouble();
                        
                        if (allergenElement.TryGetProperty("source", out var sourceElement))
                            allergen.Source = sourceElement.GetString();
                        
                        detectedAllergens.Add(allergen);
                    }
                }
            }

            // Publikacja zdarzenia detekcji alergenów
            await _eventBus.PublishAsync(new AllergenDetectionPerformedEvent
            {
                Ingredients = ingredients,
                DetectedAllergensCount = detectedAllergens.Count,
                UserId = ""
            });

            return new AllergenDetectionResult
            {
                Success = true,
                DetectedAllergens = detectedAllergens
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting allergens");
            return new AllergenDetectionResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<HealthScoreResult> CalculateHealthScoreAsync(string productId)
    {
        try
        {
            var messages = new List<OpenAIMessage>
            {
                new OpenAIMessage("system", "You are a health assessment AI. Calculate health scores for food products."),
                new OpenAIMessage("user", $"Calculate health score for product ID: {productId}. Provide score, grade and detailed assessment.")
            };

            var response = await _openAIClient.SendPromptForJsonasync(messages);
            
            double score = 0;
            string grade = "C";
            var positiveAspects = new List<string>();
            var negativeAspects = new List<string>();

            if (response.HasValue)
            {
                if (response.Value.TryGetProperty("score", out JsonElement scoreElement))
                    score = scoreElement.GetDouble();
                
                if (response.Value.TryGetProperty("grade", out JsonElement gradeElement))
                    grade = gradeElement.GetString() ?? "C";
                
                if (response.Value.TryGetProperty("positive_aspects", out JsonElement posElement) && posElement.ValueKind == JsonValueKind.Array)
                {
                    positiveAspects = posElement.EnumerateArray().Select(p => p.GetString() ?? "").ToList();
                }
                
                if (response.Value.TryGetProperty("negative_aspects", out JsonElement negElement) && negElement.ValueKind == JsonValueKind.Array)
                {
                    negativeAspects = negElement.EnumerateArray().Select(n => n.GetString() ?? "").ToList();
                }
            }

            // Publikacja zdarzenia kalkulacji health score
            await _eventBus.PublishAsync(new HealthScoreCalculatedEvent
            {
                ProductId = productId,
                Score = score,
                Grade = grade,
                UserId = ""
            });

            return new HealthScoreResult
            {
                Success = true,
                Score = score,
                Grade = grade,
                PositiveAspects = positiveAspects,
                NegativeAspects = negativeAspects
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating health score for product: {ProductId}", productId);
            return new HealthScoreResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}