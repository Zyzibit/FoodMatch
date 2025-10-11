using inzynierka.EventBus.Events;

namespace inzynierka.Products.EventBus.Events;

public class ProductViewedEvent : BaseIntegrationEvent
{
    public string ProductId { get; set; } = string.Empty;
    public DateTime ViewTime { get; set; }

    public ProductViewedEvent()
    {
        ModuleName = "Products";
    }
}