namespace Shipment.Domain.Events;

/// <summary>
/// Interface for shipment sent events
/// </summary>
public interface IShipmentSentEvent { }

/// <summary>
/// Event indicating shipment was sent successfully
/// </summary>
public record ShipmentSentEvent : IShipmentSentEvent
{
    public Guid ShipmentId { get; init; }
    public Guid OrderId { get; init; }
    public Guid UserId { get; init; }
    public string TrackingNumber { get; init; } = string.Empty;
    public decimal TotalPrice { get; init; }
    public DateTime SentAt { get; init; }
}

/// <summary>
/// Event indicating shipment sending failed
/// </summary>
public record ShipmentSendFailedEvent : IShipmentSentEvent
{
    public Guid OrderId { get; init; }
    public IEnumerable<string> Reasons { get; init; } = Enumerable.Empty<string>();
}

