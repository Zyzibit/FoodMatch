using inzynierka.EventBus.Events;

namespace inzynierka.AI.EventBus.Events;

/// <summary>
/// Zdarzenia zwi¹zane z AI
/// </summary>
public class AIAnalysisRequestedEvent : BaseIntegrationEvent
{
    public string UserId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string AnalysisType { get; set; } = string.Empty;

    public AIAnalysisRequestedEvent()
    {
        ModuleName = "AI";
    }
}

public class AIAnalysisCompletedEvent : BaseIntegrationEvent
{
    public string ProductId { get; set; } = string.Empty;
    public string AnalysisType { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
    public TimeSpan ProcessingTime { get; set; }

    public AIAnalysisCompletedEvent()
    {
        ModuleName = "AI";
    }
}

public class RecipeGeneratedEvent : BaseIntegrationEvent
{
    public string UserId { get; set; } = string.Empty;
    public List<string> Ingredients { get; set; } = new();
    public int RecipeCount { get; set; }

    public RecipeGeneratedEvent()
    {
        ModuleName = "AI";
    }
}

public class AITextGeneratedEvent : BaseIntegrationEvent
{
    public string UserId { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int TokensUsed { get; set; }
    public TimeSpan ProcessingTime { get; set; }

    public AITextGeneratedEvent()
    {
        ModuleName = "AI";
    }
}

public class AIJsonGeneratedEvent : BaseIntegrationEvent
{
    public string UserId { get; set; } = string.Empty;
    public bool IsValidJson { get; set; }
    public string Schema { get; set; } = string.Empty;

    public AIJsonGeneratedEvent()
    {
        ModuleName = "AI";
    }
}

public class NutritionalAnalysisPerformedEvent : BaseIntegrationEvent
{
    public string ProductId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public double OverallScore { get; set; }

    public NutritionalAnalysisPerformedEvent()
    {
        ModuleName = "AI";
    }
}

public class AllergenDetectionPerformedEvent : BaseIntegrationEvent
{
    public List<string> Ingredients { get; set; } = new();
    public int DetectedAllergensCount { get; set; }
    public string UserId { get; set; } = string.Empty;

    public AllergenDetectionPerformedEvent()
    {
        ModuleName = "AI";
    }
}

public class HealthScoreCalculatedEvent : BaseIntegrationEvent
{
    public string ProductId { get; set; } = string.Empty;
    public double Score { get; set; }
    public string Grade { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;

    public HealthScoreCalculatedEvent()
    {
        ModuleName = "AI";
    }
}