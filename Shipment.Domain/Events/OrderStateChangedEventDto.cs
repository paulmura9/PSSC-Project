using SharedKernel;
using System.Diagnostics.CodeAnalysis;

namespace Shipment.Domain.Events;

/// <summary>
/// DTO for deserializing OrderStateChangedEvent from Service Bus
/// Used by Shipment to consume order events from Ordering
/// Instantiated via JSON deserialization, not directly
/// </summary>
public sealed record OrderStateChangedEventDto : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid UserId { get; init; }
    public bool PremiumSubscription { get; init; }
    
    /// <summary>
    /// Order status (Placed)
    /// </summary>
    public string OrderStatus { get; init; } = "Placed";
    
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
    
    public decimal TotalPrice => Total;

    public OrderStateChangedEventDto() : base() { }
    
    public OrderStateChangedEventDto(Guid eventId, DateTime occurredAt) : base(eventId, occurredAt) { }
}
