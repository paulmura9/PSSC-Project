namespace SharedKernel;

/// <summary>
/// Event published when a shipment is created successfully
/// </summary>
public record ShipmentCreatedEvent : IntegrationEvent
{
    public Guid ShipmentId { get; init; }
    public Guid OrderId { get; init; }
    public Guid UserId { get; init; }
    public string TrackingNumber { get; init; } = string.Empty;
    public decimal TotalPrice { get; init; }
    public List<ShipmentLineItem> Lines { get; init; } = new();
}


