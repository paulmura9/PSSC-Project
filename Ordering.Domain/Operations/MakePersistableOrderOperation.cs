using Ordering.Domain.Models;
using static Ordering.Domain.Models.Order;

namespace Ordering.Domain.Operations;

/// <summary>
/// Transforms a PricedOrder into a PersistableOrder (mapped to DB model)
/// PricedOrder -> PersistableOrder
/// </summary>
public class MakePersistableOrderOperation : OrderOperation
{
    protected override Task<IOrder> OnPricedAsync(PricedOrder order, CancellationToken cancellationToken)
    {
        // Map ValidatedOrderLines to PersistableOrderLines
        var persistableLines = order.Lines
            .Select(line => new PersistableOrderLine(
                Name: line.Name.Value,
                Description: line.Description.Value,
                Category: line.Category.Value,
                Quantity: line.Quantity.Value,
                UnitPrice: line.UnitPrice.Value,
                LineTotal: line.LineTotal.Value))
            .ToList()
            .AsReadOnly();

        var persistableOrder = new PersistableOrder(
            lines: persistableLines,
            userId: order.UserId,
            street: order.Street,
            city: order.City,
            postalCode: order.PostalCode,
            phone: order.Phone,
            email: order.Email,
            deliveryNotes: order.DeliveryNotes,
            subtotal: order.Subtotal,
            discountAmount: order.DiscountAmount,
            total: order.Total,
            voucherCode: order.VoucherCode,
            premiumSubscription: order.PremiumSubscription,
            pickupMethod: order.PickupMethod.Value,
            pickupPointId: order.PickupPointId?.Value,
            paymentMethod: order.PaymentMethod.Value);

        return Task.FromResult<IOrder>(persistableOrder);
    }
}

