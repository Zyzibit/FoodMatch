using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using inzynierka.AI.Contracts;
using inzynierka.AI.Contracts.Models;

namespace inzynierka.AI.API;

[ApiController]
[Route("api/v1/ai")]
public class AIController : ControllerBase
{
    private readonly IAIContract _aiModule;
    private readonly ILogger<AIController> _logger;

    public AIController(IAIContract aiModule, ILogger<AIController> logger)
    {
        _aiModule = aiModule;
        _logger = logger;
    }

    /// <summary>
    /// Generowanie odpowiedzi tekstowej AI
    /// </summary>
    [HttpPost("generate-text")]
    [Authorize]
    public async Task<IActionResult> GenerateText([FromBody] GenerateTextRequest request)
    {
        try
        {
            var messages = request.Messages.Select(m => new AIMessage
            {
                Role = m.Role,
                Content = m.Content
            }).ToList();

            var options = new AIGenerationOptions
            {
                Model = request.Model,
                Temperature = request.Temperature,
                MaxTokens = request.MaxTokens,
                Language = request.Language
            };

            var result = await _aiModule.GenerateResponseAsync(messages, options);
            
            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new
            {
                success = true,
                response = result.Response,
                modelUsed = result.ModelUsed,
                tokensUsed = result.TokensUsed
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating AI text response");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Generowanie odpowiedzi JSON AI
    /// </summary>
    [HttpPost("generate-json")]
    [Authorize]
    public async Task<IActionResult> GenerateJson([FromBody] GenerateJsonRequest request)
    {
        try
        {
            var messages = request.Messages.Select(m => new AIMessage
            {
                Role = m.Role,
                Content = m.Content
            }).ToList();

            var result = await _aiModule.GenerateJsonAsync(messages, request.Schema);
            
            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new
            {
                success = true,
                jsonResponse = result.JsonResponse,
                isValidJson = result.IsValidJson
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating AI JSON response");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Analiza produktu
    /// </summary>
    [HttpPost("analyze-product/{productId}")]
    [Authorize]
    public async Task<IActionResult> AnalyzeProduct(string productId, [FromBody] ProductAnalysisRequest request)
    {
        try
        {
            var result = await _aiModule.AnalyzeProductAsync(productId, request.AnalysisType);
            
            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new
            {
                success = true,
                analysis = result.Analysis,
                confidenceScore = result.ConfidenceScore,
                tags = result.Tags,
                metadata = result.Metadata
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing product: {ProductId}", productId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Rekomendacje przepisów
    /// </summary>
    [HttpPost("recipe-recommendations")]
    [Authorize]
    public async Task<IActionResult> GetRecipeRecommendations([FromBody] RecipeRecommendationRequest request)
    {
        try
        {
            var preferences = request.Preferences != null ? new DietaryPreferences
            {
                IsVegetarian = request.Preferences.IsVegetarian,
                IsVegan = request.Preferences.IsVegan,
                IsGlutenFree = request.Preferences.IsGlutenFree,
                IsLactoseFree = request.Preferences.IsLactoseFree,
                Allergies = request.Preferences.Allergies ?? new List<string>(),
                DislikedIngredients = request.Preferences.DislikedIngredients ?? new List<string>(),
                CuisineType = request.Preferences.CuisineType,
                MaxCalories = request.Preferences.MaxCalories
            } : null;

            var result = await _aiModule.GetRecipeRecommendationsAsync(request.Ingredients, preferences);
            
            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new
            {
                success = true,
                recommendations = result.Recommendations
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recipe recommendations");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Analiza ¿ywieniowa
    /// </summary>
    [HttpGet("nutrition-analysis/{productId}")]
    [Authorize]
    public async Task<IActionResult> GetNutritionalAnalysis(string productId)
    {
        try
        {
            var result = await _aiModule.AnalyzeNutritionAsync(productId);
            
            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new
            {
                success = true,
                analysis = result.Analysis,
                score = result.Score,
                recommendations = result.Recommendations
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting nutritional analysis for product: {ProductId}", productId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Detekcja alergenów
    /// </summary>
    [HttpPost("detect-allergens")]
    [Authorize]
    public async Task<IActionResult> DetectAllergens([FromBody] AllergenDetectionRequest request)
    {
        try
        {
            var result = await _aiModule.DetectAllergensAsync(request.Ingredients);
            
            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new
            {
                success = true,
                detectedAllergens = result.DetectedAllergens
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting allergens");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Ocena zdrowotna produktu
    /// </summary>
    [HttpGet("health-score/{productId}")]
    [Authorize]
    public async Task<IActionResult> GetHealthScore(string productId)
    {
        try
        {
            var result = await _aiModule.CalculateHealthScoreAsync(productId);
            
            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new
            {
                success = true,
                score = result.Score,
                grade = result.Grade,
                positiveAspects = result.PositiveAspects,
                negativeAspects = result.NegativeAspects
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating health score for product: {ProductId}", productId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}

// DTOs dla API
public class GenerateTextRequest
{
    public List<MessageDto> Messages { get; set; } = new();
    public string? Model { get; set; }
    public double? Temperature { get; set; }
    public int? MaxTokens { get; set; }
    public string? Language { get; set; }
}

public class GenerateJsonRequest
{
    public List<MessageDto> Messages { get; set; } = new();
    public string? Schema { get; set; }
}

public class MessageDto
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class ProductAnalysisRequest
{
    public ProductAnalysisType AnalysisType { get; set; }
}

public class RecipeRecommendationRequest
{
    public List<string> Ingredients { get; set; } = new();
    public DietaryPreferencesDto? Preferences { get; set; }
}

public class DietaryPreferencesDto
{
    public bool IsVegetarian { get; set; }
    public bool IsVegan { get; set; }
    public bool IsGlutenFree { get; set; }
    public bool IsLactoseFree { get; set; }
    public List<string>? Allergies { get; set; }
    public List<string>? DislikedIngredients { get; set; }
    public string? CuisineType { get; set; }
    public int? MaxCalories { get; set; }
}

public class AllergenDetectionRequest
{
    public List<string> Ingredients { get; set; } = new();
}