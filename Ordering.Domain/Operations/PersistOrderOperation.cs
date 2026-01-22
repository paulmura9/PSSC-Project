using Ordering.Domain.Models;
using Ordering.Domain.Repositories;
using static Ordering.Domain.Models.Order;

namespace Ordering.Domain.Operations;

/// <summary>
/// Operation that persists a PersistableOrder to the database
/// PersistableOrder -> PersistedOrder
/// ASYNC - requires I/O (database)
/// </summary>
public class PersistOrderOperation
{
    private readonly IOrderRepository _repository;

    public PersistOrderOperation(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<IOrder> ExecuteAsync(PersistableOrder order, CancellationToken cancellationToken = default)
    {
        var orderId = await _repository.SaveOrderAsync(order, cancellationToken);

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

        return new PersistedOrder(
            orderId: orderId,
            lines: validatedLines,
            userId: CustomerId.Create(order.UserId),
            street: !string.IsNullOrWhiteSpace(order.Street) ? Street.Create(order.Street) : null,
            city: !string.IsNullOrWhiteSpace(order.City) ? City.Create(order.City) : null,
            postalCode: !string.IsNullOrWhiteSpace(order.PostalCode) ? PostalCode.Create(order.PostalCode) : null,
            phone: PhoneNumber.Create(order.Phone),
            email: !string.IsNullOrWhiteSpace(order.Email) ? EmailAddress.Create(order.Email) : null,
            subtotal: order.Subtotal,
            discountAmount: order.DiscountAmount,
            total: order.Total,
            voucherCode: !string.IsNullOrWhiteSpace(order.VoucherCode) ? VoucherCode.Create(order.VoucherCode) : null,
            premiumSubscription: order.PremiumSubscription,
            pickupMethod: new PickupMethod(order.PickupMethod),
            pickupPointId: order.PickupPointId != null ? new PickupPointId(order.PickupPointId) : null,
            paymentMethod: new PaymentMethod(order.PaymentMethod),
            createdAt: DateTime.UtcNow);
    }
}
