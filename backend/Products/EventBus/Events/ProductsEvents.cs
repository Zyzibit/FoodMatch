using inzynierka.EventBus.Events;

namespace inzynierka.Products.EventBus.Events;

/// <summary>
/// Zdarzenia zwi¹zane z produktami
/// </summary>
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

public class ProductUpdatedEvent : BaseIntegrationEvent
{
    public string ProductId { get; set; } = string.Empty;
    public Dictionary<string, object> UpdatedFields { get; set; } = new();

    public ProductUpdatedEvent()
    {
        ModuleName = "Products";
    }
}

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

public class ProductViewedEvent : BaseIntegrationEvent
{
    public string UserId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public DateTime ViewTime { get; set; }

    public ProductViewedEvent()
    {
        ModuleName = "Products";
    }
}

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