using inzynierka.EventBus.Events;

namespace inzynierka.Products.EventBus.Events;

public class ProductImportedEvent : BaseIntegrationEvent
{
    public int ImportedCount { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }

    public ProductImportedEvent()
    {
        ModuleName = "Products";
    }
}