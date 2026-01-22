using Ordering.Domain.Models;
using Ordering.Domain.Repositories;
using static Ordering.Domain.Models.Order;

namespace Ordering.Domain.Operations;

/// <summary>
/// Persists a PricedOrder to the database and returns PersistedOrder
/// PricedOrder -> PersistedOrder
/// ASYNC - requires I/O (Database)
/// </summary>
public class PersistOrderOperation
{
    private readonly IOrderRepository _repository;

    public PersistOrderOperation(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<IOrder> ExecuteAsync(PricedOrder order, CancellationToken cancellationToken = default)
    {
        var orderId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        
        var saveData = MapToSaveData(order, orderId, createdAt);
        await _repository.SaveOrderAsync(saveData, cancellationToken);

        return new PersistedOrder(
            orderId: orderId,
            lines: order.Lines,
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
            pickupMethod: order.PickupMethod,
            pickupPointId: order.PickupPointId,
            paymentMethod: order.PaymentMethod,
            createdAt: createdAt);
    }

    private static SharedKernel.Ordering.OrderSaveData MapToSaveData(PricedOrder order, Guid orderId, DateTime createdAt)
    {
        return new SharedKernel.Ordering.OrderSaveData
        {
            OrderId = orderId,
            UserId = order.UserId.Value,
            Street = order.Street?.Value,
            City = order.City?.Value,
            PostalCode = order.PostalCode?.Value,
            Phone = order.Phone.Value,
            Email = order.Email?.Value,
            DeliveryNotes = order.DeliveryNotes?.Value,
            Subtotal = order.Subtotal,
            DiscountAmount = order.DiscountAmount,
            Total = order.Total,
            VoucherCode = order.VoucherCode?.Value,
            PremiumSubscription = order.PremiumSubscription,
            PickupMethod = order.PickupMethod.Value,
            PickupPointId = order.PickupPointId?.Value,
            PaymentMethod = order.PaymentMethod.Value,
            CreatedAt = createdAt,
            Lines = order.Lines.Select(line => new SharedKernel.Ordering.OrderLineSaveData
            {
                OrderLineId = Guid.NewGuid(),
                Name = line.Name.Value,
                Description = line.Description.Value,
                Category = line.Category.Value,
                Quantity = line.Quantity.Value,
                UnitPrice = line.UnitPrice.Value,
                LineTotal = line.LineTotal.Value
            }).ToList().AsReadOnly()
        };
    }
}
