using Shipment.Domain.Models;
using static Shipment.Domain.Models.Shipment;

namespace Shipment.Domain.Operations;

/// <summary>
/// Operation that calculates shipping cost for a shipment (SYNC - pure transformation)
/// Transforms CreatedShipment -> ShippingCostCalculatedShipment
/// 
/// Shipping cost rules (in RON):
/// - Premium customers: FREE (0 RON)
/// - Regular customers based on order total:
///   - 0-3000 RON: 30 RON
///   - 3001-6000 RON: 50 RON
///   - 6001-10000 RON: 75 RON
///   - >= 10001 RON: 100 RON
/// </summary>
public sealed class CalculateShippingCostOperation : ShipmentOperation
{
    protected override IShipment OnCreated(CreatedShipment shipment)
    {
        var isPremium = shipment.PremiumSubscription;
        var shippingCost = CalculateShippingCost(shipment.TotalPrice.Value, isPremium);
        var totalWithShipping = shipment.TotalPrice.Value + shippingCost;

        return new ShippingCostCalculatedShipment(
            orderId: shipment.OrderId,
            userId: shipment.UserId,
            premiumSubscription: isPremium,
            orderTotal: shipment.TotalPrice,
            shippingCost: new Money(shippingCost),
            totalWithShipping: new Money(totalWithShipping),
            lines: shipment.Lines,
            orderPlacedAt: shipment.OrderPlacedAt,
            calculatedAt: DateTime.UtcNow);
    }

    /// <summary>
    /// Calculates shipping cost based on order total and premium status
    /// </summary>
    private static decimal CalculateShippingCost(decimal orderTotal, bool isPremium)
    {
        if (isPremium)
            return 0m;

        return orderTotal switch
        {
            <= 3000m => 30m,
            <= 6000m => 50m,
            <= 10000m => 75m,
            _ => 100m
        };
    }
}