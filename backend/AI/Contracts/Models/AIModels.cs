namespace inzynierka.AI.Contracts.Models;

public class AIMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class AIGenerationOptions
{
    public string? Model { get; set; }
    public double? Temperature { get; set; }
    public int? MaxTokens { get; set; }
    public string? Language { get; set; }
}

public class AITextResult
{
    public bool Success { get; set; }
    public string? Response { get; set; }
    public string? ModelUsed { get; set; }
    public int? TokensUsed { get; set; }
    public string? ErrorMessage { get; set; }
}

public class AIJsonResult
{
    public bool Success { get; set; }
    public string? JsonResponse { get; set; }
    public bool IsValidJson { get; set; }
    public string? ErrorMessage { get; set; }
}

public class ProductAnalysisResult
{
    public bool Success { get; set; }
    public string? Analysis { get; set; }
    public double ConfidenceScore { get; set; }
    public List<string>? Tags { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public string? ErrorMessage { get; set; }
}

public enum ProductAnalysisType
{
    Nutritional,
    Allergens,
    Ingredients,
    Environmental,
    Health,
    Dietary
}

public class RecipeRecommendationResult
{
    public bool Success { get; set; }
    public List<RecipeRecommendation> Recommendations { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public class RecipeRecommendation
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Ingredients { get; set; } = new();
    public List<string> Instructions { get; set; } = new();
    public int PrepTimeMinutes { get; set; }
    public int CookTimeMinutes { get; set; }
    public int Servings { get; set; }
    public double MatchScore { get; set; }
}

public class DietaryPreferences
{
    public bool IsVegetarian { get; set; }
    public bool IsVegan { get; set; }
    public bool IsGlutenFree { get; set; }
    public bool IsLactoseFree { get; set; }
    public List<string> Allergies { get; set; } = new();
    public List<string> DislikedIngredients { get; set; } = new();
    public string? CuisineType { get; set; }
    public int? MaxCalories { get; set; }
}

public class NutritionalAnalysisResult
{
    public bool Success { get; set; }
    public string? Analysis { get; set; }
    public NutritionScore? Score { get; set; }
    public List<string>? Recommendations { get; set; }
    public string? ErrorMessage { get; set; }
}

public class NutritionScore
{
    public double Overall { get; set; }
    public double Protein { get; set; }
    public double Fat { get; set; }
    public double Carbs { get; set; }
    public double Vitamins { get; set; }
    public double Minerals { get; set; }
}

public class AllergenDetectionResult
{
    public bool Success { get; set; }
    public List<DetectedAllergen> DetectedAllergens { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public class DetectedAllergen
{
    public string Name { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string? Source { get; set; }
    public string? Severity { get; set; }
}

public class HealthScoreResult
{
    public bool Success { get; set; }
    public double Score { get; set; }
    public string? Grade { get; set; }
    public List<string>? PositiveAspects { get; set; }
    public List<string>? NegativeAspects { get; set; }
    public string? ErrorMessage { get; set; }
}