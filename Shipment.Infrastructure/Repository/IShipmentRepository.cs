namespace Shipment.Infrastructure.Persistence;

/// <summary>
/// Interface for Shipment persistence operations
/// Shipment only reacts to events from Ordering - it doesn't own the Cancel/Modify/Return workflows
/// </summary>
public interface IShipmentRepository
{
    /// <summary>
    /// Saves a new shipment with its lines (reacting to OrderPlaced)
    /// </summary>
    Task SaveShipmentAsync(ShipmentEntity shipment, IEnumerable<ShipmentLineEntity> lines, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a shipment by order ID (for looking up related shipment)
    /// </summary>
    Task<ShipmentEntity?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a shipment by its ID
    /// </summary>
    Task<ShipmentEntity?> GetByIdAsync(Guid shipmentId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates shipment status (reacting to OrderCancelled, OrderModified, OrderReturned)
    /// </summary>
    Task UpdateStatusAsync(Guid shipmentId, string newStatus, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Marks shipment as cancelled when order is cancelled (if not yet dispatched)
    /// </summary>
    Task CancelByOrderIdAsync(Guid orderId, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates shipment status by order ID (for Cancel/Return events)
    /// </summary>
    Task UpdateStatusByOrderIdAsync(Guid orderId, string newStatus, CancellationToken cancellationToken = default);
}

