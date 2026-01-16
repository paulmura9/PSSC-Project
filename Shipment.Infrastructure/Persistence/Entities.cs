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
    public string TrackingNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Pending, Shipped, Delivered
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation property for shipment lines
    public ICollection<ShipmentLineEntity> Lines { get; set; } = new List<ShipmentLineEntity>();
}

/// <summary>
/// Database entity for Shipment Line
/// </summary>
public class ShipmentLineEntity
{
    public Guid ShipmentLineId { get; set; }
    public Guid ShipmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }

    // Navigation property
    public ShipmentEntity Shipment { get; set; } = null!;
}

