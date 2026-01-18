using Shipment.Domain.Models;
using static Shipment.Domain.Models.Shipment;

namespace Shipment.Domain.Operations;

/// <summary>
/// Operation that calculates shipping cost for a shipment
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
public class CalculateShippingCostOperation
{
    /// <summary>
    /// Calculates shipping cost based on order total and premium status
    /// </summary>
    public static decimal CalculateShippingCost(decimal orderTotal, bool isPremium)
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

    /// <summary>
    /// Gets shipping cost description for logging
    /// </summary>
    public static string GetShippingCostDescription(decimal orderTotal, bool isPremium)
    {
        if (isPremium)
            return "Premium customer - FREE shipping";

        var cost = CalculateShippingCost(orderTotal, isPremium);
        var range = orderTotal switch
        {
            <= 3000m => "0-3000 RON",
            <= 6000m => "3001-6000 RON",
            <= 10000m => "6001-10000 RON",
            _ => ">=10001 RON"
        };

        return $"Order total {orderTotal:N2} RON ({range}) -> Shipping: {cost:N2} RON";
    }

    /// <summary>
    /// Transforms CreatedShipment to ShippingCostCalculatedShipment
    /// </summary>
    public Task<IShipment> ExecuteAsync(
        CreatedShipment shipment,
        bool isPremium,
        CancellationToken cancellationToken = default)
    {
        var shippingCost = CalculateShippingCost(shipment.TotalPrice.Value, isPremium);
        var totalWithShipping = shipment.TotalPrice.Value + shippingCost;

        var result = new ShippingCostCalculatedShipment(
            orderId: shipment.OrderId,
            userId: shipment.UserId,
            premiumSubscription: isPremium,
            orderTotal: shipment.TotalPrice,
            shippingCost: new Money(shippingCost),
            totalWithShipping: new Money(totalWithShipping),
            lines: shipment.Lines,
            orderPlacedAt: shipment.OrderPlacedAt,
            calculatedAt: DateTime.UtcNow);

        return Task.FromResult<IShipment>(result);
    }
}

