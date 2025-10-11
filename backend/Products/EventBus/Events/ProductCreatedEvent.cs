using inzynierka.EventBus.Events;

namespace inzynierka.Products.EventBus.Events;

public class ProductCreatedEvent : BaseIntegrationEvent
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;

    public ProductCreatedEvent()
    {
        ModuleName = "Products";
    }
}