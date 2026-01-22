namespace Shipment.Infrastructure.Persistence;

/// <summary>
/// Database entity for Shipment
/// </summary>
public class ShipmentEntity
{
    public Guid ShipmentId { get; set; }
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal TotalWithShipping { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Pending, Shipped, Delivered
    public DateTime CreatedAt { get; set; }

    // Navigation property for shipment lines
    public ICollection<ShipmentLineEntity> Lines { get; set; } = new List<ShipmentLineEntity>();
}

