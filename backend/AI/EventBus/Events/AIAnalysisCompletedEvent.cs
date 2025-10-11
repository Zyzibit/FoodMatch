using inzynierka.EventBus.Events;

namespace inzynierka.AI.EventBus.Events;

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