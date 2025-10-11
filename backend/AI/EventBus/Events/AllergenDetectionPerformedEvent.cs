using inzynierka.EventBus.Events;

namespace inzynierka.AI.EventBus.Events;

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