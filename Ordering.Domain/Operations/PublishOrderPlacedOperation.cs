using SharedKernel;
using Ordering.Domain.Models;
using static Ordering.Domain.Models.Order;

namespace Ordering.Domain.Operations;

/// <summary>
/// Publishes a PersistedOrder to the event bus and returns PublishedOrder
/// PersistedOrder -> PublishedOrder
/// ASYNC - requires I/O (Service Bus)
/// </summary>
public class PublishOrderPlacedOperation
{
    private readonly IEventBus _eventBus;
    private readonly IEventHistoryService _eventHistory;

    public PublishOrderPlacedOperation(IEventBus eventBus, IEventHistoryService eventHistory)
    {
        _eventBus = eventBus;
        _eventHistory = eventHistory;
    }

    public async Task<IOrder> ExecuteAsync(PersistedOrder order, CancellationToken cancellationToken = default)
    {
        // Map domain objects to OrderStateChangedEvent with Status="Placed"
        var stateChangedEvent = new OrderStateChangedEvent
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            OrderStatus = OrderStatus.Placed,
            OrderId = order.OrderId,
            UserId = order.UserId.Value,
            PremiumSubscription = order.PremiumSubscription,
            Subtotal = order.Subtotal,
            DiscountAmount = order.DiscountAmount,
            Total = order.Total,
            VoucherCode = order.VoucherCode?.Value,
            Lines = order.Lines.Select(line => new LineItemDto
            {
                Name = line.Name.Value,
                Description = line.Description.Value,
                Category = line.Category.Value,
                Quantity = line.Quantity.Value,
                UnitPrice = line.UnitPrice.Value,
                LineTotal = line.LineTotal.Value
            }).ToList(),
            Street = order.Street?.Value,
            City = order.City?.Value,
            PostalCode = order.PostalCode?.Value,
            Phone = order.Phone.Value,
            Email = order.Email?.Value,
            PickupMethod = order.PickupMethod.Value,
            PickupPointId = order.PickupPointId?.Value,
            PaymentMethod = order.PaymentMethod.Value
        };

        // Publish to Service Bus
        await _eventBus.PublishAsync(TopicNames.Orders, stateChangedEvent, cancellationToken);

        // Save event to CSV history
        await _eventHistory.SaveEventAsync(
            stateChangedEvent,
            eventType: "OrderStateChanged:Placed",
            source: "Ordering.Api",
            orderId: order.OrderId.ToString(),
            status: "Published"
        );

        // Return PublishedOrder state
        return new PublishedOrder(
            orderId: order.OrderId,
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
            publishedAt: DateTime.UtcNow);
    }
}
