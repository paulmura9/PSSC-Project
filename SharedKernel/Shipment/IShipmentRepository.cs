namespace SharedKernel.Shipment;

/// <summary>
/// Interface for shipment repository operations
/// Port in SharedKernel, implemented in Infrastructure
/// </summary>
public interface IShipmentRepository
{
    Task SaveShipmentAsync(ShipmentSaveData shipment, CancellationToken cancellationToken = default);
    Task<ShipmentQueryResult?> GetByIdAsync(Guid shipmentId, CancellationToken cancellationToken = default);
    Task<ShipmentQueryResult?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(Guid shipmentId, string newStatus, CancellationToken cancellationToken = default);
    Task UpdateStatusByOrderIdAsync(Guid orderId, string newStatus, CancellationToken cancellationToken = default);
}

/// <summary>
/// DTO for saving shipment data to repository
/// </summary>
public record ShipmentSaveData
{
    public Guid ShipmentId { get; init; }
    public Guid OrderId { get; init; }
    public Guid UserId { get; init; }
    public string TrackingNumber { get; init; } = string.Empty;
    public decimal TotalPrice { get; init; }
    public decimal ShippingCost { get; init; }
    public decimal TotalWithShipping { get; init; }
    public string Status { get; init; } = "Created";
    public IReadOnlyCollection<ShipmentLineSaveData> Lines { get; init; } = Array.Empty<ShipmentLineSaveData>();
}

/// <summary>
/// DTO for saving shipment line data
/// </summary>
public record ShipmentLineSaveData
{
    public Guid ShipmentLineId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Category { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal LineTotal { get; init; }
}

/// <summary>
/// DTO for shipment data returned from repository
/// </summary>
public record ShipmentQueryResult
{
    public Guid ShipmentId { get; init; }
    public Guid OrderId { get; init; }
    public Guid UserId { get; init; }
    public string TrackingNumber { get; init; } = string.Empty;
    public decimal TotalPrice { get; init; }
    public decimal ShippingCost { get; init; }
    public decimal TotalWithShipping { get; init; }
    public string Status { get; init; } = "Created";
    public DateTime CreatedAt { get; init; }
}

