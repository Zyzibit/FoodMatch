using inzynierka.EventBus.Events;

namespace inzynierka.AI.EventBus.Events;

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