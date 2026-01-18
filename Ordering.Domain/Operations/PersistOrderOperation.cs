using Ordering.Domain.Models;
using static Ordering.Domain.Models.Order;

namespace Ordering.Domain.Operations;

/// <summary>
/// Operation that persists a PersistableOrder to the database
/// PersistableOrder -> PersistedOrder
/// </summary>
public class PersistOrderOperation : OrderOperationWithState<IPersistence>
{
    protected override async Task<IOrder> OnPersistableAsync(PersistableOrder order, IPersistence persistence, CancellationToken cancellationToken)
    {
        var orderId = await persistence.SaveOrderAsync(order, cancellationToken);

        // Convert PersistableOrderLines back to ValidatedOrderLines for PersistedOrder
        var validatedLines = order.Lines
            .Select(line => ValidatedOrderLine.Create(
                line.Name,
                line.Description,
                line.Category,
                line.Quantity,
                line.UnitPrice))
            .ToList()
            .AsReadOnly();

        var persistedOrder = new PersistedOrder(
            orderId: orderId,
            lines: validatedLines,
            userId: order.UserId,
            street: order.Street,
            city: order.City,
            postalCode: order.PostalCode,
            phone: order.Phone,
            email: order.Email,
            subtotal: order.Subtotal,
            discountAmount: order.DiscountAmount,
            total: order.Total,
            voucherCode: order.VoucherCode,
            premiumSubscription: order.PremiumSubscription,
            pickupMethod: new PickupMethod(order.PickupMethod),
            pickupPointId: order.PickupPointId != null ? new PickupPointId(order.PickupPointId) : null,
            paymentMethod: new PaymentMethod(order.PaymentMethod),
            createdAt: DateTime.UtcNow);

        return persistedOrder;
    }
}

