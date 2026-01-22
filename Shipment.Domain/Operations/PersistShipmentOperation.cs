using SharedKernel.Shipment;
using static Shipment.Domain.Models.Shipment;

namespace Shipment.Domain.Operations;

/// <summary>
/// Persists a ScheduledShipment or DispatchedShipment to the database and returns PersistedShipment
/// ASYNC - requires I/O (Database)
/// </summary>
public class PersistShipmentOperation
{
    private readonly IShipmentRepository _repository;

    public PersistShipmentOperation(IShipmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<IShipment> ExecuteAsync(IShipment shipment, CancellationToken cancellationToken = default)
    {
        // Extract common data based on shipment state
        var (shipmentId, orderId, userId, totalPrice, shippingCost, totalWithShipping, lines, trackingNumber, orderPlacedAt, status) = shipment switch
        {
            DispatchedShipment dispatched => (
                dispatched.ShipmentId, dispatched.OrderId, dispatched.UserId,
                dispatched.TotalPrice, dispatched.ShippingCost, dispatched.TotalWithShipping,
                dispatched.Lines, dispatched.TrackingNumber, dispatched.OrderPlacedAt,
                "Dispatched"),
            ScheduledShipment scheduled => (
                scheduled.ShipmentId, scheduled.OrderId, scheduled.UserId,
                scheduled.TotalPrice, scheduled.ShippingCost, scheduled.TotalWithShipping,
                scheduled.Lines, scheduled.TrackingNumber, scheduled.OrderPlacedAt,
                "Scheduled"),
            _ => throw new InvalidOperationException($"Unexpected shipment state: {shipment.GetType().Name}")
        };

        //VO
        var persisted = new PersistedShipment(
            shipmentId: shipmentId,
            orderId: orderId,
            userId: userId,
            totalPrice: totalPrice,
            shippingCost: shippingCost,
            totalWithShipping: totalWithShipping,
            lines: lines,
            trackingNumber: trackingNumber,
            orderPlacedAt: orderPlacedAt,
            persistedAt: DateTime.UtcNow);

        // Map to save data DTO (VO->string)
        var saveData = new ShipmentSaveData
        {
            ShipmentId = persisted.ShipmentId,
            OrderId = persisted.OrderId,
            UserId = persisted.UserId,
            TrackingNumber = persisted.TrackingNumber.Value,
            TotalPrice = persisted.TotalPrice.Value,
            ShippingCost = persisted.ShippingCost.Value,
            TotalWithShipping = persisted.TotalWithShipping.Value,
            Status = status,
            Lines = persisted.Lines.Select(l => new ShipmentLineSaveData
            {
                ShipmentLineId = Guid.NewGuid(),
                Name = l.Name.Value,
                Description = l.Description.Value,
                Category = l.Category.Value,
                Quantity = l.Quantity.Value,
                UnitPrice = l.UnitPrice.Value,
                LineTotal = l.LineTotal.Value
            }).ToList()
        };

        await _repository.SaveShipmentAsync(saveData, cancellationToken);

        return persisted;  //return VO
    }
}

