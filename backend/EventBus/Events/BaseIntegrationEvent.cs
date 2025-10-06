namespace inzynierka.EventBus.Events;

/// <summary>
/// Bazowa klasa dla wszystkich zdarzeñ w systemie
/// </summary>
public abstract class BaseIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
    public string ModuleName { get; protected set; } = string.Empty;
}