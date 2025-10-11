using inzynierka.EventBus.Events;

namespace inzynierka.AI.EventBus.Events;

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