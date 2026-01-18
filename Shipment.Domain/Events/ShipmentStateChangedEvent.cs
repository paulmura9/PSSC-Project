using SharedKernel;

namespace Shipment.Domain.Events;

/// <summary>
/// Single event emitted by Shipment context (similar to OrderStateChangedEvent)
/// Contains ShipmentStatus to indicate what happened
/// This is the ONLY event published to "shipments" topic
/// </summary>
public record ShipmentStateChangedEvent : IntegrationEvent
{
    /// <summary>
    /// Current status: Created, Scheduled, Dispatched, Delivered, Cancelled, Returned
    /// </summary>
    public string ShipmentStatus { get; init; } = string.Empty;
    
    public Guid ShipmentId { get; init; }
    public Guid OrderId { get; init; }
    public Guid UserId { get; init; }
    
    /// <summary>
    /// Whether customer has premium subscription (free shipping)
    /// </summary>
    public bool PremiumSubscription { get; init; }
    
    /// <summary>
    /// Payment method from the order (CashOnDelivery, CardOnDelivery, CardOnline)
    /// </summary>
    public string PaymentMethod { get; init; } = "CashOnDelivery";
    
    // Pricing fields (in RON)
    public decimal Subtotal { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal TotalAfterDiscount { get; init; }
    public decimal ShippingCost { get; init; }
    public decimal TotalWithShipping { get; init; }
    
    public string TrackingNumber { get; init; } = string.Empty;
    
    /// <summary>
    /// Shipment lines
    /// </summary>
    public List<ShipmentLineDto> Lines { get; init; } = new();
    
    /// <summary>
    /// Reason for Cancel/Return (optional)
    /// </summary>
    public string? Reason { get; init; }

    public ShipmentStateChangedEvent() : base(Guid.NewGuid(), DateTime.UtcNow) { }
}

/// <summary>
/// DTO for shipment line in events - uses primitive types for serialization
/// </summary>
public record ShipmentLineDto
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Category { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal LineTotal { get; init; }
}

/// <summary>
/// Shipment status values
/// </summary>
public static class ShipmentStatus
{
    public const string Created = "Created";
    public const string Scheduled = "Scheduled";
    public const string Dispatched = "Dispatched";
    public const string Delivered = "Delivered";
    public const string Cancelled = "Cancelled";
    public const string Returned = "Returned";
    public const string Priority = "Priority"; // For premium customers
}

