using inzynierka.EventBus.Events;

namespace inzynierka.Products.EventBus.Events;

public class NutritionInfoAccessedEvent : BaseIntegrationEvent
{
    public string ProductId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime AccessTime { get; set; }

    public NutritionInfoAccessedEvent()
    {
        ModuleName = "Products";
    }
}