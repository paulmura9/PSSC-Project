using Microsoft.Extensions.Logging;
using Ordering.Domain.Events;
using Ordering.Domain.Models;
using Ordering.Domain.Operations;

namespace Ordering.Domain.Workflows;

/// <summary>
/// Workflow for returning orders
/// Business Rule: Can only return after shipment was delivered
/// </summary>
public class ReturnOrderWorkflow
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderEventPublisher _eventPublisher;
    private readonly ILogger<ReturnOrderWorkflow> _logger;

    public ReturnOrderWorkflow(
        IOrderRepository orderRepository,
        IOrderEventPublisher eventPublisher,
        ILogger<ReturnOrderWorkflow> logger)
    {
        _orderRepository = orderRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<IOrderReturnedEvent> ExecuteAsync(
        ReturnOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting order return for Order: {OrderId}", command.OrderId);

            // Get current order from database
            var orderEntity = await _orderRepository.GetByIdAsync(command.OrderId, cancellationToken);

            if (orderEntity == null)
            {
                _logger.LogWarning("Order {OrderId} not found", command.OrderId);
                return new OrderReturnFailedEvent(command.OrderId, $"Order {command.OrderId} not found");
            }

            // Business rule: Cannot return a cancelled order
            if (orderEntity.Status == "Cancelled")
            {
                _logger.LogWarning("Order {OrderId} is already cancelled, cannot return", command.OrderId);
                return new OrderReturnFailedEvent(command.OrderId, "Cannot return a cancelled order");
            }

            // Business rule: Cannot return an already returned order
            if (orderEntity.Status == "Returned")
            {
                _logger.LogWarning("Order {OrderId} is already returned", command.OrderId);
                return new OrderReturnFailedEvent(command.OrderId, "Order has already been returned");
            }

            // Validate return reason
            if (string.IsNullOrWhiteSpace(command.ReturnReason))
            {
                return new OrderReturnFailedEvent(command.OrderId, "Return reason is required");
            }

            if (command.ReturnReason.Length > 500)
            {
                return new OrderReturnFailedEvent(command.OrderId, "Return reason cannot exceed 500 characters");
            }

            // Update order status in database
            await _orderRepository.UpdateStatusAsync(command.OrderId, "Returned", cancellationToken);

            _logger.LogInformation("Order {OrderId} marked as returned successfully", command.OrderId);

            // Create OrderStateChangedEvent with Status=Returned
            var stateChangedEvent = new OrderStateChangedEvent
            {
                EventId = Guid.NewGuid(),
                OccurredAt = DateTime.UtcNow,
                OrderStatus = OrderStatus.Returned,
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
                Reason = command.ReturnReason
            };

            // Publish single event for Shipment (optional return shipment) and Invoicing (credit note)
            await _eventPublisher.PublishOrderStateChangedAsync(stateChangedEvent, cancellationToken);

            return new OrderReturnSucceededEvent(command.OrderId, orderEntity.UserId, command.ReturnReason, DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error returning order {OrderId}: {Message}", command.OrderId, ex.Message);
            return new OrderReturnFailedEvent(command.OrderId, ex.Message);
        }
    }
}

