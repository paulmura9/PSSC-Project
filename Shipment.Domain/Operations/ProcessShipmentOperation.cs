using static Shipment.Domain.Models.Shipment;

namespace Shipment.Domain.Operations;

/// <summary>
/// Processes a shipment - transforms ValidatedShipment to ProcessedShipment (generates tracking number)
/// </summary>
public class ProcessShipmentOperation : ShipmentOperation
{
    protected override Task<IShipment> OnValidatedAsync(ValidatedShipment shipment, object state, CancellationToken cancellationToken)
    {
        var trackingNumber = GenerateTrackingNumber();
        var shipmentId = Guid.NewGuid();

        return Task.FromResult<IShipment>(new ProcessedShipment(
            shipmentId,
            shipment.OrderId,
            shipment.UserId,
            shipment.TotalPrice,
            shipment.Lines,
            trackingNumber,
            shipment.OrderPlacedAt));
    }

    private static string GenerateTrackingNumber()
    {
        return $"TRACK-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }
}

