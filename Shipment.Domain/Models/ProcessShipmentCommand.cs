namespace Shipment.Domain.Models;

/// <summary>
/// Command to process a shipment
/// </summary>
public record ProcessShipmentCommand(
    Guid OrderId,
    Guid UserId,
    decimal TotalPrice,
    IReadOnlyCollection<ShipmentLine> Lines,
    DateTime OrderPlacedAt);

