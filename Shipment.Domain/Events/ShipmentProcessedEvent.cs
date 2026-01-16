namespace Shipment.Domain.Events;

/// <summary>
/// Events that occur during shipment processing
/// </summary>
public static class ShipmentProcessedEvent
{
    public abstract record ShipmentProcessEvent;

    public record ShipmentProcessSucceededEvent : ShipmentProcessEvent
    {
        public Guid ShipmentId { get; init; }
        public string TrackingNumber { get; init; } = string.Empty;
    }

    public record ShipmentProcessFailedEvent : ShipmentProcessEvent
    {
        public IEnumerable<string> Reasons { get; init; } = Enumerable.Empty<string>();
    }
}

