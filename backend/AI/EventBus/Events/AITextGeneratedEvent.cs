using inzynierka.EventBus.Events;

namespace inzynierka.AI.EventBus.Events;

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