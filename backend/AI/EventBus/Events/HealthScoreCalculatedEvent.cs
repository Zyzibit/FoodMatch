using inzynierka.EventBus.Events;

namespace inzynierka.AI.EventBus.Events;

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