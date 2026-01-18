using SharedKernel;

namespace Ordering.Domain.Events;

/// <summary>
/// Single event emitted by Ordering context
/// Contains OrderStatus to indicate what happened (Placed, Cancelled, Returned, Modified)
/// This is the ONLY event published to "orders" topic
/// </summary>
public record OrderStateChangedEvent : IntegrationEvent
{
    /// <summary>
    /// Current status of the order: Placed, Cancelled, Returned, Modified
    /// </summary>
    public string OrderStatus { get; init; } = string.Empty;
    
    public Guid OrderId { get; init; }
    public Guid UserId { get; init; }
    
    /// <summary>
    /// Whether customer has premium subscription (free shipping)
    /// </summary>
    public bool PremiumSubscription { get; init; }
    
    // Pricing fields
    public decimal Subtotal { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal Total { get; init; }
    public string? VoucherCode { get; init; }
    
    // For backwards compatibility
    public decimal TotalPrice => Total;
    
    /// <summary>
    /// Order lines (for Placed/Modified events)
    /// </summary>
    public List<OrderLineDto> Lines { get; init; } = new();
    
    // Address info (nullable for pickup orders)
    public string? Street { get; init; }
    public string? City { get; init; }
    public string? PostalCode { get; init; }
    public string Phone { get; init; } = string.Empty;
    public string? Email { get; init; }
    
    // Pickup/Delivery method
    public string PickupMethod { get; init; } = "HomeDelivery";
    public string? PickupPointId { get; init; }
    
    // Payment fields
    public string PaymentMethod { get; init; } = "CashOnDelivery";
    
    /// <summary>
    /// Reason for Cancel/Return (optional)
    /// </summary>
    public string? Reason { get; init; }

    public OrderStateChangedEvent() : base(Guid.NewGuid(), DateTime.UtcNow) { }
}

/// <summary>
/// DTO for order line in events - uses primitive types for serialization
/// </summary>
public record OrderLineDto
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal LineTotal { get; init; }
}

/// <summary>
/// Order status values
/// </summary>
public static class OrderStatus
{
    public const string Placed = "Placed";
    public const string Cancelled = "Cancelled";
    public const string Returned = "Returned";
    public const string Modified = "Modified";
}

