﻿using SharedKernel;
using SharedKernel.Messaging;
using static Shipment.Domain.Models.Shipment;

namespace Shipment.Domain.Operations;

/// <summary>
/// Publishes a PersistedShipment to the event bus and returns PublishedShipment
/// PersistedShipment -> PublishedShipment
/// ASYNC - requires I/O (Service Bus)
/// </summary>
public class PublishShipmentOperation
{
    private readonly IEventBus _eventBus;

    public PublishShipmentOperation(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task<IShipment> ExecuteAsync(
        PersistedShipment shipment, 
        bool isPremium,
        string paymentMethod,
        decimal subtotal,
        decimal discountAmount,
        CancellationToken cancellationToken = default)
    {
        var status = isPremium ? "Dispatched" : "Scheduled";

        var stateChangedEvent = new ShipmentStateChangedEvent
        {
            ShipmentId = shipment.ShipmentId,
            OrderId = shipment.OrderId,
            UserId = shipment.UserId,
            PremiumSubscription = isPremium,
            PaymentMethod = paymentMethod,
            TrackingNumber = shipment.TrackingNumber.Value,
            ShipmentState = status,
            Subtotal = subtotal,
            DiscountAmount = discountAmount,
            TotalAfterDiscount = shipment.TotalPrice.Value,
            ShippingCost = shipment.ShippingCost.Value,
            TotalWithShipping = shipment.TotalWithShipping.Value,
            Lines = shipment.Lines.Select(l => new LineItemDto(
                l.Name.Value,
                l.Description.Value,
                l.Category.Value,
                l.Quantity.Value,
                l.UnitPrice.Value,
                l.LineTotal.Value
            )).ToList(),
            OccurredAt = DateTime.UtcNow
        };

        await _eventBus.PublishAsync(TopicNames.Shipments, stateChangedEvent, cancellationToken);

        return new PublishedShipment(shipment, DateTime.UtcNow);
    }
}

