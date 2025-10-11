using inzynierka.EventBus.Events;

namespace inzynierka.Products.EventBus.Events;

public class ProductUpdatedEvent : BaseIntegrationEvent
{
    public string ProductId { get; set; } = string.Empty;
    public Dictionary<string, object> UpdatedFields { get; set; } = new();

    public ProductUpdatedEvent()
    {
        ModuleName = "Products";
    }
}