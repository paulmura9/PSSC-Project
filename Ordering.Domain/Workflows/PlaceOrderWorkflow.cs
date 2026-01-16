using Microsoft.Extensions.Logging;
using Ordering.Domain.Events;
using Ordering.Domain.Models;
using Ordering.Domain.Operations;
using static Ordering.Domain.Models.Order;
using static Ordering.Domain.Events.OrderPlacedEvent;

namespace Ordering.Domain.Workflows;

/// <summary>
/// Workflow that orchestrates the place order process
/// </summary>
public class PlaceOrderWorkflow
{
    private readonly ValidateOrderOperation _validateOperation;
    private readonly PriceOrderOperation _priceOperation;
    private readonly PersistOrderOperation _persistOperation;
    private readonly IPersistence _persistence;
    private readonly ILogger<PlaceOrderWorkflow> _logger;

    public PlaceOrderWorkflow(
        ValidateOrderOperation validateOperation,
        PriceOrderOperation priceOperation,
        PersistOrderOperation persistOperation,
        IPersistence persistence,
        ILogger<PlaceOrderWorkflow> logger)
    {
        _validateOperation = validateOperation;
        _priceOperation = priceOperation;
        _persistOperation = persistOperation;
        _persistence = persistence;
        _logger = logger;
    }

    /// <summary>
    /// Executes the place order workflow
    /// </summary>
    public async Task<IOrderPlacedEvent> ExecuteAsync(PlaceOrderCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting order placement for user {UserId}", command.UnvalidatedOrder.UserId);

            // Step 1: Validate
            IOrder order = await _validateOperation.TransformAsync(command.UnvalidatedOrder, cancellationToken);
            _logger.LogInformation("Validation result: {OrderType}", order.GetType().Name);

            if (order is Order.InvalidOrder)
            {
                _logger.LogWarning("Order validation failed");
                return order.ToEvent();
            }

            // Step 2: Price
            order = await _priceOperation.TransformAsync(order, cancellationToken);
            _logger.LogInformation("Pricing result: {OrderType}", order.GetType().Name);

            
            if (order is Order.PricedOrder pricedOrder)
            {
                 order = await _persistOperation.TransformAsync(pricedOrder, _persistence, cancellationToken);
                 _logger.LogInformation("Persist result: {OrderType}", order.GetType().Name);
            }
            
            // Evaluate the state of the entity and generate the appropriate event
            return order.ToEvent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while placing order: {Message}", ex.Message);
            return new OrderPlaceFailedEvent(new[] { "Unexpected error" });
        }
    }
}

