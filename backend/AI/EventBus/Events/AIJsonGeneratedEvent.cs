using inzynierka.EventBus.Events;

namespace inzynierka.AI.EventBus.Events;

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