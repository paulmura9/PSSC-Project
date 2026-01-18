using Microsoft.Extensions.Logging;
using Ordering.Domain.Events;
using Ordering.Domain.Models;
using Ordering.Domain.Operations;

namespace Ordering.Domain.Workflows;

/// <summary>
/// Workflow for cancelling orders
/// Business Rule: Can only cancel if order is not yet shipped (Dispatched)
/// </summary>
public class CancelOrderWorkflow
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderEventPublisher _eventPublisher;
    private readonly ILogger<CancelOrderWorkflow> _logger;

    public CancelOrderWorkflow(
        IOrderRepository orderRepository,
        IOrderEventPublisher eventPublisher,
        ILogger<CancelOrderWorkflow> logger)
    {
        _orderRepository = orderRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<IOrderCancelledEvent> ExecuteAsync(
        CancelOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting order cancellation for Order: {OrderId}", command.OrderId);

            // Get current order from database
            var orderEntity = await _orderRepository.GetByIdAsync(command.OrderId, cancellationToken);

            if (orderEntity == null)
            {
                _logger.LogWarning("Order {OrderId} not found", command.OrderId);
                return new OrderCancellationFailedEvent(command.OrderId, $"Order {command.OrderId} not found");
            }

            // Business rule: Cannot cancel an already cancelled order
            if (orderEntity.Status == "Cancelled")
            {
                _logger.LogWarning("Order {OrderId} is already cancelled", command.OrderId);
                return new OrderCancellationFailedEvent(command.OrderId, "Order is already cancelled");
            }

            // Business rule: Cannot cancel a returned order
            if (orderEntity.Status == "Returned")
            {
                _logger.LogWarning("Order {OrderId} is already returned, cannot cancel", command.OrderId);
                return new OrderCancellationFailedEvent(command.OrderId, "Cannot cancel a returned order");
            }

            // Validate cancellation reason
            if (string.IsNullOrWhiteSpace(command.Reason))
            {
                return new OrderCancellationFailedEvent(command.OrderId, "Cancellation reason is required");
            }

            // Update order status in database
            await _orderRepository.UpdateStatusAsync(command.OrderId, "Cancelled", cancellationToken);

            _logger.LogInformation("Order {OrderId} cancelled successfully", command.OrderId);

            // Create OrderStateChangedEvent with Status=Cancelled
            var stateChangedEvent = new OrderStateChangedEvent
            {
                EventId = Guid.NewGuid(),
                OccurredAt = DateTime.UtcNow,
                OrderStatus = OrderStatus.Cancelled,
                OrderId = command.OrderId,
                UserId = orderEntity.UserId,
                Subtotal = orderEntity.Subtotal,
                DiscountAmount = orderEntity.DiscountAmount,
                Total = orderEntity.Total,
                VoucherCode = orderEntity.VoucherCode,
                Street = orderEntity.Street,
                City = orderEntity.City,
                PostalCode = orderEntity.PostalCode,
                Phone = orderEntity.Phone,
                Email = orderEntity.Email,
                Reason = command.Reason
            };

            // Publish single event for Shipment and Invoicing to react
            await _eventPublisher.PublishOrderStateChangedAsync(stateChangedEvent, cancellationToken);

            return new OrderCancelledSucceededEvent(command.OrderId, orderEntity.UserId, command.Reason, DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling order {OrderId}: {Message}", command.OrderId, ex.Message);
            return new OrderCancellationFailedEvent(command.OrderId, ex.Message);
        }
    }
}

