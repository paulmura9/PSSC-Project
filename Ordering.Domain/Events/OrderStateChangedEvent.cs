using System.Text;
using SharedKernel;
using Ordering.Domain.Models;

namespace Ordering.Domain.Events;

/// <summary>
/// Single event emitted by Ordering context
/// Contains OrderStatus to indicate what happened (Placed, Cancelled, Returned, Modified)
/// This is the ONLY event published to "orders" topic
/// </summary>
/// pentru SB
public record OrderStateChangedEvent() : IntegrationEvent(Guid.NewGuid(), DateTime.UtcNow)
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
    public List<LineItemDto> Lines { get; init; } = new();
    
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


    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine($"===== Order {OrderStatus} =====");
        sb.AppendLine($"Order ID: {OrderId}");
        sb.AppendLine($"User ID: {UserId}");
        sb.AppendLine($"Premium: {PremiumSubscription}");
        sb.AppendLine($"Pickup: {PickupMethod}" + (PickupPointId != null ? $" ({PickupPointId})" : ""));
        sb.AppendLine($"Payment: {PaymentMethod}");
        
        if (!string.IsNullOrEmpty(Street))
            sb.AppendLine($"Address: {Street}, {City} {PostalCode}");
        
        sb.AppendLine($"Phone: {Phone}");
        
        if (Lines.Count > 0)
        {
            sb.AppendLine("Lines:");
            foreach (var line in Lines)
                sb.AppendLine(line.ToString());
        }
        
        sb.AppendLine($"Subtotal: {Subtotal:C}");
        if (DiscountAmount > 0)
            sb.AppendLine($"Discount: -{DiscountAmount:C}" + (!string.IsNullOrEmpty(VoucherCode) ? $" ({VoucherCode})" : ""));
        sb.AppendLine($"Total: {Total:C}");
        
        if (!string.IsNullOrEmpty(Reason))
            sb.AppendLine($"Reason: {Reason}");
        
        sb.AppendLine("=============================");
        return sb.ToString();
    }
}


