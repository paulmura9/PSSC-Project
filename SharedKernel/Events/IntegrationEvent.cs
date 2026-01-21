namespace SharedKernel;

/// <summary>
/// Base class for integration events published via event bus
/// All events between bounded contexts should extend this
/// </summary>
public abstract record IntegrationEvent
{
    public Guid EventId { get; init; }
    public DateTime OccurredAt { get; init; }

    protected IntegrationEvent()
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
    }

    protected IntegrationEvent(Guid eventId, DateTime occurredAt)
    {
        EventId = eventId;
        OccurredAt = occurredAt;
    }
}

