using inzynierka.AI.Contracts.Models;

namespace inzynierka.AI.Contracts;

/// <summary>
/// Kontrakt dla modu³u AI - definiuje interfejs komunikacji
/// </summary>
public interface IAIContract
{
    Task<AITextResult> GenerateResponseAsync(List<AIMessage> messages, AIGenerationOptions? options = null);
    Task<AIJsonResult> GenerateJsonAsync(List<AIMessage> messages, string? schema = null);
    Task<ProductAnalysisResult> AnalyzeProductAsync(string productId, ProductAnalysisType analysisType);
    Task<RecipeRecommendationResult> GetRecipeRecommendationsAsync(List<string> ingredients, DietaryPreferences? preferences = null);
    Task<NutritionalAnalysisResult> AnalyzeNutritionAsync(string productId);
    Task<AllergenDetectionResult> DetectAllergensAsync(List<string> ingredients);
    Task<HealthScoreResult> CalculateHealthScoreAsync(string productId);
}