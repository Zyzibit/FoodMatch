using inzynierka.EventBus.Events;

namespace inzynierka.Products.EventBus.Events;

public class ProductSearchedEvent : BaseIntegrationEvent
{
    public string UserId { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public int ResultsCount { get; set; }

    public ProductSearchedEvent()
    {
        ModuleName = "Products";
    }
}