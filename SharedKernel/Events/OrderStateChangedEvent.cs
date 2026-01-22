using System.Text;

namespace SharedKernel;

/// <summary>
/// Event emitted by Ordering context when order state changes
/// Contains OrderStatus = "Placed" when order is successfully placed
/// Published to "orders" topic, consumed by Shipment
/// </summary>
/// in json pt bus
public record OrderStateChangedEvent() : IntegrationEvent(Guid.NewGuid(), DateTime.UtcNow)
{
    /// <summary>
    /// Current status of the order: Placed
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
    /// Order lines
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
        
        sb.AppendLine("=============================");
        return sb.ToString();
    }
}

