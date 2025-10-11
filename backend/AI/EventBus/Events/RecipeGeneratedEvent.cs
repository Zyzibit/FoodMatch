using inzynierka.EventBus.Events;

namespace inzynierka.AI.EventBus.Events;

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