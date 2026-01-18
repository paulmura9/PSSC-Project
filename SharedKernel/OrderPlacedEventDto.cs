namespace SharedKernel;

/// <summary>
/// DTO for Order Placed events - uses primitive types for JSON serialization
/// This is the "contract" between Ordering and Shipment services
/// </summary>
public sealed record OrderPlacedEventDto : IntegrationEvent
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public decimal TotalPrice { get; set; }
    public List<OrderLineEventDto> Lines { get; set; } = new();
    
    public OrderPlacedEventDto() : base() { }
    
    public OrderPlacedEventDto(Guid eventId, DateTime occurredAt) : base(eventId, occurredAt) { }
}


