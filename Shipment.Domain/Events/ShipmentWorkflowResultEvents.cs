using System.Text;

namespace Shipment.Domain.Events;

/// <summary>
/// Interface for shipment workflow result events
/// </summary>
public interface IShipmentWorkflowResult { }

/// <summary>
/// Event indicating shipment was created successfully (internal workflow result)
/// </summary>
public record ShipmentCreatedSuccessEvent : IShipmentWorkflowResult
{
    public Guid ShipmentId { get; init; }
    public Guid OrderId { get; init; }
    public Guid UserId { get; init; }
    public string TrackingNumber { get; init; } = string.Empty;
    public decimal TotalPrice { get; init; }
    public DateTime CreatedAt { get; init; }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine("===== Shipment Created =====");
        sb.AppendLine($"Shipment ID: {ShipmentId}");
        sb.AppendLine($"Order ID: {OrderId}");
        sb.AppendLine($"User ID: {UserId}");
        sb.AppendLine($"Tracking: {TrackingNumber}");
        sb.AppendLine($"Total: {TotalPrice:C}");
        sb.AppendLine($"Created At: {CreatedAt:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine("============================");
        return sb.ToString();
    }
}

/// <summary>
/// Event indicating shipment creation failed (internal workflow result)
/// </summary>
public record ShipmentCreatedFailedEvent : IShipmentWorkflowResult
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
