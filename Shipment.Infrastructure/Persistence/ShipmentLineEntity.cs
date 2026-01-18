namespace Shipment.Infrastructure.Persistence;

/// <summary>
/// Database entity for Shipment Line
/// </summary>
public class ShipmentLineEntity
{
    public Guid ShipmentLineId { get; set; }
    public Guid ShipmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }

    // Navigation property
    public ShipmentEntity Shipment { get; set; } = null!;
}

