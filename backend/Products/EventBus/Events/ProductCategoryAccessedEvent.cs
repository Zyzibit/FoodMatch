using inzynierka.EventBus.Events;

namespace inzynierka.Products.EventBus.Events;

public class ProductCategoryAccessedEvent : BaseIntegrationEvent
{
    public string CategoryName { get; set; } = string.Empty;
    public int ProductCount { get; set; }
    public string UserId { get; set; } = string.Empty;

    public ProductCategoryAccessedEvent()
    {
        ModuleName = "Products";
    }
}