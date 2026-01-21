using System.Text;

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

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine("===== Shipment Sent =====");
        sb.AppendLine($"Shipment ID: {ShipmentId}");
        sb.AppendLine($"Order ID: {OrderId}");
        sb.AppendLine($"User ID: {UserId}");
        sb.AppendLine($"Tracking: {TrackingNumber}");
        sb.AppendLine($"Total: {TotalPrice:C}");
        sb.AppendLine($"Sent At: {SentAt:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine("=========================");
        return sb.ToString();
    }
}

/// <summary>
/// Event indicating shipment sending failed
/// </summary>
public record ShipmentSendFailedEvent : IShipmentSentEvent
{
    public Guid OrderId { get; init; }
    public IEnumerable<string> Reasons { get; init; } = Enumerable.Empty<string>();

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine("===== Shipment Failed =====");
        sb.AppendLine($"Order ID: {OrderId}");
        sb.AppendLine($"Reasons: {string.Join(", ", Reasons)}");
        sb.AppendLine("===========================");
        return sb.ToString();
    }
}

