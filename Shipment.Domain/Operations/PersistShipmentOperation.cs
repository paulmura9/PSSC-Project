using Shipment.Infrastructure.Persistence;
using static Shipment.Domain.Models.Shipment;

namespace Shipment.Domain.Operations;

/// <summary>
/// Persists a shipment to the database - transforms ProcessedShipment to PersistedShipment
/// </summary>
public class PersistShipmentOperation : ShipmentOperation<IShipmentRepository>
{
    public override async Task<IShipment> TransformAsync(IShipment shipment, IShipmentRepository repository, CancellationToken cancellationToken)
    {
        return shipment switch
        {
            ProcessedShipment processed => await OnProcessedAsync(processed, repository, cancellationToken),
            _ => shipment
        };
    }

    private async Task<IShipment> OnProcessedAsync(ProcessedShipment shipment, IShipmentRepository repository, CancellationToken cancellationToken)
    {
        var shipmentEntity = new ShipmentEntity
        {
            ShipmentId = shipment.ShipmentId,
            OrderId = shipment.OrderId,
            UserId = shipment.UserId,
            TotalPrice = shipment.TotalPrice,
            TrackingNumber = shipment.TrackingNumber,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        var lineEntities = shipment.Lines.Select(line => new ShipmentLineEntity
        {
            ShipmentLineId = Guid.NewGuid(),
            ShipmentId = shipment.ShipmentId,
            Name = line.Name,
            Quantity = line.Quantity,
            UnitPrice = line.UnitPrice,
            LineTotal = line.LineTotal
        }).ToList();

        await repository.SaveShipmentAsync(shipmentEntity, lineEntities, cancellationToken);

        return new PersistedShipment(
            shipment.ShipmentId,
            shipment.OrderId,
            shipment.UserId,
            shipment.TotalPrice,
            shipment.Lines,
            shipment.TrackingNumber,
            shipment.OrderPlacedAt,
            DateTime.UtcNow);
    }
}

