namespace Shipment.Domain.Operations;

/// <summary>
/// Interface for shipment repository operations
/// Port in Domain layer, implemented in Infrastructure
/// </summary>
public interface IShipmentRepository
{
    /// <summary>
    /// Gets a shipment by its ID
    /// </summary>
    Task<ShipmentQueryResult?> GetByIdAsync(Guid shipmentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a shipment by order ID
    /// </summary>
    Task<ShipmentQueryResult?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the status of a shipment by order ID
    /// </summary>
    Task UpdateStatusByOrderIdAsync(Guid orderId, string newStatus, CancellationToken cancellationToken = default);
}

/// <summary>
/// DTO for shipment data returned from repository
/// Used to avoid dependency on Infrastructure entities in Domain
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

