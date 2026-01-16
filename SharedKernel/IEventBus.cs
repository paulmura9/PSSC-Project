namespace SharedKernel;

/// <summary>
/// Interface for the event bus
/// </summary>
public interface IEventBus
{
    Task PublishAsync<TEvent>(string topicName, TEvent @event, CancellationToken cancellationToken = default) 
        where TEvent : IntegrationEvent;
}

