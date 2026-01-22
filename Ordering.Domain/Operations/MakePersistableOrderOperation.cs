using Ordering.Domain.Models;
using static Ordering.Domain.Models.Order;

namespace Ordering.Domain.Operations;

/// <summary>
/// Transforms a PricedOrder into a PersistableOrder (mapped to DB model)
/// PricedOrder -> PersistableOrder
/// SYNC - pure transformation (VO -> primitives, no I/O)
/// </summary>
public class MakePersistableOrderOperation : OrderOperation
{
    protected override IOrder OnPriced(PricedOrder order)
    {
        // Map ValidatedOrderLines to PersistableOrderLines (vo->primitive)
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

        return new PersistableOrder(
            lines: persistableLines,
            userId: order.UserId.Value,
            street: order.Street?.Value,
            city: order.City?.Value,
            postalCode: order.PostalCode?.Value,
            phone: order.Phone.Value,
            email: order.Email?.Value,
            deliveryNotes: order.DeliveryNotes?.Value,
            subtotal: order.Subtotal,
            discountAmount: order.DiscountAmount,
            total: order.Total,
            voucherCode: order.VoucherCode?.Value,
            premiumSubscription: order.PremiumSubscription,
            pickupMethod: order.PickupMethod.Value,
            pickupPointId: order.PickupPointId?.Value,
            paymentMethod: order.PaymentMethod.Value);
    }
}
