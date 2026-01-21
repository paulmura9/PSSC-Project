namespace Ordering.Infrastructure.Persistence;

/// <summary>
/// Entity representing an order in the database
/// </summary>
public class OrderEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    
    // Address fields (nullable for pickup orders)
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? DeliveryNotes { get; set; }
    
    // Pricing fields
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total { get; set; }
    public string? VoucherCode { get; set; }
    
    // Pickup/Delivery method
    public string PickupMethod { get; set; } = "HomeDelivery";
    public string? PickupPointId { get; set; }
    
    // Payment fields
    public string PaymentMethod { get; set; } = "CashOnDelivery";
    
    // For backwards compatibility - computed property (not mapped to DB)
    public decimal TotalPrice => Total;
    
    public string Status { get; set; } = "Sent";
    public DateTime CreatedAt { get; set; }

    public ICollection<OrderLineEntity> Lines { get; set; } = new List<OrderLineEntity>();
}

