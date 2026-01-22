using static Shipment.Domain.Models.Shipment;

namespace Shipment.Domain.Operations;

/// <summary>
/// Operation that dispatches a shipment - marks as sent out for delivery 
/// Transforms ScheduledShipment -> DispatchedShipment
/// </summary>
public sealed class DispatchShipmentOperation : ShipmentOperation
{
    protected override IShipment OnScheduled(ScheduledShipment shipment)
    {
        return new DispatchedShipment(
            shipmentId: shipment.ShipmentId,
            orderId: shipment.OrderId,
            userId: shipment.UserId,
            totalPrice: shipment.TotalPrice,
            shippingCost: shipment.ShippingCost,
            totalWithShipping: shipment.TotalWithShipping,
            lines: shipment.Lines,
            trackingNumber: shipment.TrackingNumber,
            orderPlacedAt: shipment.OrderPlacedAt,
            dispatchedAt: DateTime.UtcNow);
    }
}

