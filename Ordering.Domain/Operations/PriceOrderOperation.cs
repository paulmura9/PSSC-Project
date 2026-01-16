using Ordering.Domain.Models;
using static Ordering.Domain.Models.Order;

namespace Ordering.Domain.Operations;

/// <summary>
/// Operation that calculates the total price for a validated order
/// </summary>
public class PriceOrderOperation : OrderOperation
{
    protected override Task<IOrder> OnValidatedAsync(ValidatedOrder order, CancellationToken cancellationToken)
    {
        var totalPrice = order.Lines.Sum(line => line.LineTotal);

        var pricedOrder = new PricedOrder(
            order.Lines,
            order.UserId,
            order.DeliveryAddress,
            order.PostalCode,
            order.Phone,
            order.CardNumber,
            order.Expiry,
            totalPrice);

        return Task.FromResult<IOrder>(pricedOrder);
    }
}

