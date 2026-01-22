using Microsoft.Extensions.Logging;
using Ordering.Domain.Events;
using Ordering.Domain.Models;
using Ordering.Domain.Operations;
using static Ordering.Domain.Models.Order;

namespace Ordering.Domain.Workflows;

/// <summary>
/// Workflow that orchestrates the place order process
/// Pipeline: Unvalidated -> Validate -> Price -> Persist -> Publish
/// </summary>
public class PlaceOrderWorkflow
{
    private readonly ValidateOrderOperation _validateOperation;
    private readonly PriceOrderOperation _priceOperation;
    private readonly PersistOrderOperation _persistOperation;
    private readonly PublishOrderPlacedOperation _publishOperation;
    private readonly ILogger<PlaceOrderWorkflow> _logger;

    public PlaceOrderWorkflow(
        ValidateOrderOperation validateOperation,
        PriceOrderOperation priceOperation,
        PersistOrderOperation persistOperation,
        PublishOrderPlacedOperation publishOperation,
        ILogger<PlaceOrderWorkflow> logger)
    {
        _validateOperation = validateOperation;
        _priceOperation = priceOperation;
        _persistOperation = persistOperation;
        _publishOperation = publishOperation;
        _logger = logger;
    }

    /// <summary>
    /// Executes the place order workflow
    /// Pipeline: UnvalidatedOrder -> ValidatedOrder -> PricedOrder -> PersistedOrder -> PublishedOrder
    /// </summary>
    public async Task<IOrderPlacedEvent> ExecuteAsync(PlaceOrderCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting order placement for user {UserId}", command.UnvalidatedOrder.UserId);

            // Step 1: Validate - SYNC (pure validation, no I/O)
            // UnvalidatedOrder -> ValidatedOrder | InvalidOrder
            IOrder order = _validateOperation.Transform(command.UnvalidatedOrder);
            _logger.LogInformation("State: {OrderType}", order.GetType().Name);

            if (order is InvalidOrder)
            {
                _logger.LogWarning("Order validation failed");
                return order.ToEvent();
            }

            // Step 2: Price with optional voucher - ASYNC (DB lookup for voucher)
            // ValidatedOrder -> PricedOrder | InvalidOrder
            if (order is ValidatedOrder validatedOrder)
            {
                order = await _priceOperation.ExecuteAsync(validatedOrder, command.VoucherCode, cancellationToken);
                _logger.LogInformation("State: {OrderType}", order.GetType().Name);

                if (order is InvalidOrder invalidOrder)
                {
                    _logger.LogWarning("Order pricing/voucher validation failed");
                    return invalidOrder.ToEvent();
                }
            }

            // Step 3: Persist to database - ASYNC (VO -> primitive mapping done internally)
            // PricedOrder -> PersistedOrder
            if (order is PricedOrder pricedOrder)
            {
                order = await _persistOperation.ExecuteAsync(pricedOrder, cancellationToken);
                _logger.LogInformation("State: {OrderType}", order.GetType().Name);
            }

            // Step 4: Publish to Service Bus - ASYNC 
            // PersistedOrder -> PublishedOrder
            if (order is PersistedOrder persistedOrder)
            {
                order = await _publishOperation.ExecuteAsync(persistedOrder, cancellationToken);
                _logger.LogInformation("State: {OrderType}", order.GetType().Name);
            }

            // Final state: PublishedOrder -> OrderPlacedEvent
            return order.ToEvent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while placing order: {Message}", ex.Message);
            return new OrderPlaceFailedEvent(new[] { "Unexpected error" });
        }
    }
}
