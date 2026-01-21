using Microsoft.Extensions.Logging;
using Ordering.Domain.Events;
using Ordering.Domain.Models;
using Ordering.Domain.Operations;
using static Ordering.Domain.Models.Order;

namespace Ordering.Domain.Workflows;

/// <summary>
/// Workflow that orchestrates the place order process
/// Pipeline: Unvalidated -> Validate -> Price -> MakePersistable -> Persist -> Publish
/// </summary>
public class PlaceOrderWorkflow
{
    private readonly ValidateOrderOperation _validateOperation;
    private readonly PriceOrderOperation _priceOperation;
    private readonly MakePersistableOrderOperation _makePersistableOperation;
    private readonly PersistOrderOperation _persistOperation;
    private readonly PublishOrderPlacedOperation _publishOperation;
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderEventPublisher _eventPublisher;
    private readonly ILogger<PlaceOrderWorkflow> _logger;

    public PlaceOrderWorkflow(
        ValidateOrderOperation validateOperation,
        PriceOrderOperation priceOperation,
        MakePersistableOrderOperation makePersistableOperation,
        PersistOrderOperation persistOperation,
        PublishOrderPlacedOperation publishOperation,
        IOrderRepository orderRepository,
        IOrderEventPublisher eventPublisher,
        ILogger<PlaceOrderWorkflow> logger)
    {
        _validateOperation = validateOperation;
        _priceOperation = priceOperation;
        _makePersistableOperation = makePersistableOperation;
        _persistOperation = persistOperation;
        _publishOperation = publishOperation;
        _orderRepository = orderRepository;
        _eventPublisher = eventPublisher;
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

            // Step 1: Validate (UnvalidatedOrder -> ValidatedOrder or InvalidOrder)
            IOrder order = await _validateOperation.TransformAsync(command.UnvalidatedOrder, cancellationToken);
            _logger.LogInformation("Validation result: {OrderType}", order.GetType().Name);

            if (order is InvalidOrder)
            {
                _logger.LogWarning("Order validation failed");
                return order.ToEvent();
            }

            // Step 2: Price with optional voucher (ValidatedOrder -> PricedOrder or InvalidOrder)
            if (order is ValidatedOrder validatedOrder)
            {
                order = await _priceOperation.ExecuteAsync(validatedOrder, command.VoucherCode, cancellationToken);
                _logger.LogInformation("Pricing result: {OrderType}", order.GetType().Name);

                if (order is InvalidOrder invalidOrder)
                {
                    _logger.LogWarning("Order pricing/voucher validation failed");
                    return invalidOrder.ToEvent();
                }
            }

            // Step 3: Make Persistable (PricedOrder -> PersistableOrder)
            order = await _makePersistableOperation.TransformAsync(order, cancellationToken);
            _logger.LogInformation("MakePersistable result: {OrderType}", order.GetType().Name);

            // Step 4: Persist (PersistableOrder -> PersistedOrder)
            order = await _persistOperation.TransformAsync(order, _orderRepository, cancellationToken);
            _logger.LogInformation("Persist result: {OrderType}", order.GetType().Name);

            // Step 5: Publish (PersistedOrder -> PublishedOrder)
            order = await _publishOperation.TransformAsync(order, _eventPublisher, cancellationToken);
            _logger.LogInformation("Publish result: {OrderType}", order.GetType().Name);

            // Return the appropriate event based on final state
            return order.ToEvent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while placing order: {Message}", ex.Message);
            return new OrderPlaceFailedEvent(new[] { "Unexpected error" });
        }
    }
}

