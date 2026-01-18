using Microsoft.Extensions.Logging;
using Ordering.Domain.Events;
using Ordering.Domain.Models;
using Ordering.Domain.Operations;
using static Ordering.Domain.Models.Order;

namespace Ordering.Domain.Workflows;

/// <summary>
/// Workflow for modifying orders
/// Business Rule: Can only modify if order is not yet shipped (Dispatched)
/// Modifies: lines, address, contact info
/// </summary>
public class ModifyOrderWorkflow
{
    private readonly IOrderRepository _orderRepository;
    private readonly ValidateOrderOperation _validateOperation;
    private readonly PriceOrderOperation _priceOperation;
    private readonly IOrderEventPublisher _eventPublisher;
    private readonly ILogger<ModifyOrderWorkflow> _logger;

    public ModifyOrderWorkflow(
        IOrderRepository orderRepository,
        ValidateOrderOperation validateOperation,
        PriceOrderOperation priceOperation,
        IOrderEventPublisher eventPublisher,
        ILogger<ModifyOrderWorkflow> logger)
    {
        _orderRepository = orderRepository;
        _validateOperation = validateOperation;
        _priceOperation = priceOperation;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<IOrderModifiedEvent> ExecuteAsync(
        ModifyOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting order modification for Order: {OrderId}", command.OrderId);

            // Get current order from database
            var orderEntity = await _orderRepository.GetByIdAsync(command.OrderId, cancellationToken);

            if (orderEntity == null)
            {
                _logger.LogWarning("Order {OrderId} not found", command.OrderId);
                return new OrderModificationFailedEvent(command.OrderId, $"Order {command.OrderId} not found");
            }

            // Create unvalidated order with new data for re-validation
            // Keep existing pickup and payment settings
            var unvalidatedOrder = new Order.UnvalidatedOrder(
                command.Lines,
                orderEntity.UserId,
                command.Street,
                command.City,
                command.PostalCode,
                command.Phone,
                command.Email,
                command.DeliveryNotes,
                false, // PremiumSubscription - keep from original or command
                orderEntity.PickupMethod,
                orderEntity.PickupPointId,
                orderEntity.PaymentMethod);

            // Step 1: Re-validate the order
            var validatedResult = await _validateOperation.TransformAsync(unvalidatedOrder, cancellationToken);

            if (validatedResult is Order.InvalidOrder invalid)
            {
                _logger.LogWarning("Order modification validation failed for Order: {OrderId}", command.OrderId);
                return new OrderModificationFailedEvent(command.OrderId, invalid.Reasons);
            }

            if (validatedResult is not Order.ValidatedOrder validated)
            {
                return new OrderModificationFailedEvent(command.OrderId, "Failed to validate modified order");
            }

            // Step 2: Re-price the order (keep existing voucher if any)
            var pricedResult = await _priceOperation.ExecuteAsync(validated, orderEntity.VoucherCode, cancellationToken);

            if (pricedResult is Order.InvalidOrder invalidPriced)
            {
                return new OrderModificationFailedEvent(command.OrderId, invalidPriced.Reasons);
            }

            if (pricedResult is not Order.PricedOrder priced)
            {
                return new OrderModificationFailedEvent(command.OrderId, "Failed to price modified order");
            }

            // Step 3: Update order in database
            await _orderRepository.UpdateOrderAsync(
                command.OrderId,
                command.Street,
                command.City,
                command.PostalCode,
                command.Phone,
                command.Email,
                priced.Subtotal,
                priced.DiscountAmount,
                priced.Total,
                priced.VoucherCode,
                priced.Lines,
                cancellationToken);

            _logger.LogInformation("Order {OrderId} modified successfully", command.OrderId);

            // Create OrderStateChangedEvent with Status=Modified
            var stateChangedEvent = new OrderStateChangedEvent
            {
                EventId = Guid.NewGuid(),
                OccurredAt = DateTime.UtcNow,
                OrderStatus = OrderStatus.Modified,
                OrderId = command.OrderId,
                UserId = orderEntity.UserId,
                Subtotal = priced.Subtotal,
                DiscountAmount = priced.DiscountAmount,
                Total = priced.Total,
                VoucherCode = priced.VoucherCode,
                Lines = priced.Lines.Select(l => new OrderLineDto
                {
                    Name = l.Name.Value,
                    Description = l.Description.Value,
                    Category = l.Category.Value,
                    Quantity = l.Quantity.Value,
                    UnitPrice = l.UnitPrice.Value,
                    LineTotal = l.LineTotal.Value
                }).ToList(),
                Street = command.Street,
                City = command.City,
                PostalCode = command.PostalCode,
                Phone = command.Phone,
                Email = command.Email,
                PickupMethod = priced.PickupMethod.Value,
                PickupPointId = priced.PickupPointId?.Value,
                PaymentMethod = priced.PaymentMethod.Value
            };

            // Publish single event for Shipment and Invoicing to react
            await _eventPublisher.PublishOrderStateChangedAsync(stateChangedEvent, cancellationToken);

            return new OrderModifiedSucceededEvent(
                command.OrderId,
                orderEntity.UserId,
                priced.Total,
                DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error modifying order {OrderId}: {Message}", command.OrderId, ex.Message);
            return new OrderModificationFailedEvent(command.OrderId, ex.Message);
        }
    }
}

