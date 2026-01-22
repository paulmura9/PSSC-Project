namespace SharedKernel;

/// <summary>
/// Single event emitted by Shipment context
/// Contains ShipmentState to indicate the logistics stage
/// Contract between Shipment and Invoicing services
/// </summary>
public sealed record ShipmentStateChangedEvent : IntegrationEvent
{
    /// <summary>
    /// Current state: Scheduled, Priority
    /// </summary>
    public string ShipmentState { get; init; } = string.Empty;
    
    public Guid ShipmentId { get; init; }
    public Guid OrderId { get; init; }
    public Guid UserId { get; init; }
    public bool PremiumSubscription { get; init; }
    
    /// <summary>
    /// Payment method from the order (CashOnDelivery, CardOnDelivery, CardOnline)
    /// </summary>
    public string PaymentMethod { get; init; } = "CashOnDelivery";
    
    public string TrackingNumber { get; init; } = string.Empty;
    
    /// <summary>
    /// Subtotal before discount (sum of line totals)
    /// </summary>
    public decimal Subtotal { get; init; }
    
    /// <summary>
    /// Discount amount from voucher
    /// </summary>
    public decimal DiscountAmount { get; init; }
    
    /// <summary>
    /// Total after discount (Subtotal - DiscountAmount)
    /// </summary>
    public decimal TotalAfterDiscount { get; init; }
    
    /// <summary>
    /// Shipping cost (0 for premium, 30-100 RON for regular)
    /// </summary>
    public decimal ShippingCost { get; init; }
    
    /// <summary>
    /// Total with shipping (TotalAfterDiscount + ShippingCost)
    /// </summary>
    public decimal TotalWithShipping { get; init; }
    
    // Legacy property for backwards compatibility
    public decimal TotalPrice => Subtotal;
    
    public List<LineItemDto> Lines { get; init; } = new();

    public ShipmentStateChangedEvent() : base(Guid.NewGuid(), DateTime.UtcNow) { }
    //generaza guid si data automat, din interfata
    public ShipmentStateChangedEvent(Guid eventId, DateTime occurredAt) : base(eventId, occurredAt) { }
}

