namespace SharedKernel;

/// <summary>
/// DTO for Order Placed events - uses primitive types for JSON serialization
/// Contract between Ordering and Shipment services
/// </summary>
public sealed record OrderPlacedEventDto : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid UserId { get; init; }
    public bool PremiumSubscription { get; init; }
    
    /// <summary>
    /// Subtotal before discount
    /// </summary>
    public decimal Subtotal { get; init; }
    
    /// <summary>
    /// Discount amount from voucher
    /// </summary>
    public decimal DiscountAmount { get; init; }
    
    /// <summary>
    /// Total after discount (Subtotal - DiscountAmount)
    /// </summary>
    public decimal Total { get; init; }
    
    /// <summary>
    /// Voucher code used (if any)
    /// </summary>
    public string? VoucherCode { get; init; }
    
    /// <summary>
    /// Payment method (CashOnDelivery, CardOnDelivery, CardOnline)
    /// </summary>
    public string PaymentMethod { get; init; } = "CashOnDelivery";
    
    /// <summary>
    /// Pickup method (HomeDelivery, EasyBoxPickup, PostOfficePickup)
    /// </summary>
    public string PickupMethod { get; init; } = "HomeDelivery";
    
    /// <summary>
    /// Pickup point ID (for EasyBox/PostOffice)
    /// </summary>
    public string? PickupPointId { get; init; }
    
    public List<LineItemDto> Lines { get; init; } = new();
    
    // Legacy property for backwards compatibility
    public decimal TotalPrice => Total;

    public OrderPlacedEventDto() : base() { }
    
    public OrderPlacedEventDto(Guid eventId, DateTime occurredAt) : base(eventId, occurredAt) { }
}

