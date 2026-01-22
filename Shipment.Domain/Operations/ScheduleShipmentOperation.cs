using Shipment.Domain.Models;
using static Shipment.Domain.Models.Shipment;

namespace Shipment.Domain.Operations;

/// <summary>
/// Operation that schedules a shipment - assigns tracking number (SYNC - pure transformation)
/// Transforms ShippingCostCalculatedShipment -> ScheduledShipment
/// </summary>
public sealed class ScheduleShipmentOperation : ShipmentOperation
{
    protected override IShipment OnShippingCostCalculated(ShippingCostCalculatedShipment shipment)
    {
        var shipmentId = Guid.NewGuid();
        var trackingNumber = TrackingNumber.Generate(); //generez tracking no

        return new ScheduledShipment(
            shipmentId: shipmentId,
            orderId: shipment.OrderId,
            userId: shipment.UserId,
            totalPrice: shipment.OrderTotal,
            shippingCost: shipment.ShippingCost,
            totalWithShipping: shipment.TotalWithShipping,
            lines: shipment.Lines,
            trackingNumber: trackingNumber,
            orderPlacedAt: shipment.OrderPlacedAt,
            scheduledAt: DateTime.UtcNow);
    }
}

