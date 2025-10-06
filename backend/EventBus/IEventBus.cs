using inzynierka.EventBus.Events;

namespace inzynierka.EventBus;

/// <summary>
/// Interfejs dla szyny zdarzeñ - centralne zarz¹dzanie komunikacj¹ miêdzy modu³ami
/// </summary>
public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event) where TEvent : BaseIntegrationEvent;
    void Subscribe<TEvent, THandler>() 
        where TEvent : BaseIntegrationEvent 
        where THandler : class, IEventHandler<TEvent>;
    void Unsubscribe<TEvent, THandler>() 
        where TEvent : BaseIntegrationEvent 
        where THandler : class, IEventHandler<TEvent>;
}

/// <summary>
/// Interfejs dla handlerów zdarzeñ
/// </summary>
public interface IEventHandler<in TEvent> where TEvent : BaseIntegrationEvent
{
    Task HandleAsync(TEvent @event);
}

/// <summary>
/// In-memory implementacja szyny zdarzeñ
/// </summary>
public class InMemoryEventBus : IEventBus
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InMemoryEventBus> _logger;
    private readonly Dictionary<Type, List<Type>> _handlers = new();
    private readonly object _lock = new();

    public InMemoryEventBus(IServiceProvider serviceProvider, ILogger<InMemoryEventBus> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : BaseIntegrationEvent
    {
        var eventType = typeof(TEvent);
        
        _logger.LogInformation("Publishing event {EventType} with ID {EventId}", 
            eventType.Name, @event.Id);

        lock (_lock)
        {
            if (!_handlers.ContainsKey(eventType))
            {
                _logger.LogWarning("No handlers registered for event type {EventType}", eventType.Name);
                return;
            }
        }

        var handlerTypes = _handlers[eventType];
        var tasks = new List<Task>();

        foreach (var handlerType in handlerTypes)
        {
            try
            {
                var handler = _serviceProvider.GetService(handlerType);
                if (handler is IEventHandler<TEvent> eventHandler)
                {
                    tasks.Add(eventHandler.HandleAsync(@event));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating handler {HandlerType} for event {EventType}", 
                    handlerType.Name, eventType.Name);
            }
        }

        try
        {
            await Task.WhenAll(tasks);
            _logger.LogInformation("Successfully processed event {EventType} with {HandlerCount} handlers", 
                eventType.Name, tasks.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing event {EventType}", eventType.Name);
            throw;
        }
    }

    public void Subscribe<TEvent, THandler>()
        where TEvent : BaseIntegrationEvent
        where THandler : class, IEventHandler<TEvent>
    {
        var eventType = typeof(TEvent);
        var handlerType = typeof(THandler);

        lock (_lock)
        {
            if (!_handlers.ContainsKey(eventType))
            {
                _handlers[eventType] = new List<Type>();
            }

            if (!_handlers[eventType].Contains(handlerType))
            {
                _handlers[eventType].Add(handlerType);
                _logger.LogInformation("Subscribed handler {HandlerType} to event {EventType}", 
                    handlerType.Name, eventType.Name);
            }
        }
    }

    public void Unsubscribe<TEvent, THandler>()
        where TEvent : BaseIntegrationEvent
        where THandler : class, IEventHandler<TEvent>
    {
        var eventType = typeof(TEvent);
        var handlerType = typeof(THandler);

        lock (_lock)
        {
            if (_handlers.ContainsKey(eventType))
            {
                _handlers[eventType].Remove(handlerType);
                _logger.LogInformation("Unsubscribed handler {HandlerType} from event {EventType}", 
                    handlerType.Name, eventType.Name);

                if (_handlers[eventType].Count == 0)
                {
                    _handlers.Remove(eventType);
                }
            }
        }
    }
}