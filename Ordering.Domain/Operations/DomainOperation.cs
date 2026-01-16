using Ordering.Domain.Exceptions;
using Ordering.Domain.Models;
using static Ordering.Domain.Models.Order;

namespace Ordering.Domain.Operations;

/// <summary>
/// Base class for domain operations following the lab pattern
/// </summary>
/// <typeparam name="TEntity">The entity type being operated on</typeparam>
/// <typeparam name="TState">The state/dependency type for the operation</typeparam>
/// <typeparam name="TResult">The result type of the operation</typeparam>
public abstract class DomainOperation<TEntity, TState, TResult>
{
    /// <summary>
    /// Transforms the entity using the provided state
    /// </summary>
    public abstract Task<TResult> TransformAsync(TEntity entity, TState state, CancellationToken cancellationToken);
}

/// <summary>
/// Order operation with a state dependency
/// </summary>
/// <typeparam name="TState">The state/dependency type for the operation</typeparam>
public abstract class OrderOperation<TState> : DomainOperation<IOrder, TState, IOrder>
{
    public override async Task<IOrder> TransformAsync(IOrder order, TState state, CancellationToken cancellationToken)
    {
        return order switch
        {
            Order.UnvalidatedOrder unvalidated => await OnUnvalidatedAsync(unvalidated, state, cancellationToken),
            Order.ValidatedOrder validated => await OnValidatedAsync(validated, state, cancellationToken),
            Order.InvalidOrder invalid => await OnInvalidAsync(invalid, state, cancellationToken),
            Order.PricedOrder priced => await OnPricedAsync(priced, state, cancellationToken),
            Order.PersistedOrder persisted => await OnPersistedAsync(persisted, state, cancellationToken),
            Order.PublishedOrder published => await OnPublishedAsync(published, state, cancellationToken),
            _ => throw new InvalidOrderStateException($"Unknown order state: {order.GetType().Name}")
        };
    }

    protected virtual Task<IOrder> OnUnvalidatedAsync(Order.UnvalidatedOrder order, TState state, CancellationToken cancellationToken)
    {
        return Task.FromResult<IOrder>(order);
    }

    protected virtual Task<IOrder> OnValidatedAsync(Order.ValidatedOrder order, TState state, CancellationToken cancellationToken)
    {
        return Task.FromResult<IOrder>(order);
    }

    protected virtual Task<IOrder> OnInvalidAsync(Order.InvalidOrder order, TState state, CancellationToken cancellationToken)
    {
        return Task.FromResult<IOrder>(order);
    }

    protected virtual Task<IOrder> OnPricedAsync(Order.PricedOrder order, TState state, CancellationToken cancellationToken)
    {
        return Task.FromResult<IOrder>(order);
    }

    protected virtual Task<IOrder> OnPersistedAsync(Order.PersistedOrder order, TState state, CancellationToken cancellationToken)
    {
        return Task.FromResult<IOrder>(order);
    }

    protected virtual Task<IOrder> OnPublishedAsync(Order.PublishedOrder order, TState state, CancellationToken cancellationToken)
    {
        return Task.FromResult<IOrder>(order);
    }
}

/// <summary>
/// Order operation without state dependency
/// </summary>
public abstract class OrderOperation : OrderOperation<object>
{
    public Task<IOrder> TransformAsync(IOrder order, CancellationToken cancellationToken)
    {
        return TransformAsync(order, null!, cancellationToken);
    }

    protected virtual Task<IOrder> OnUnvalidatedAsync(Order.UnvalidatedOrder order, CancellationToken cancellationToken)
    {
        return Task.FromResult<IOrder>(order);
    }

    protected virtual Task<IOrder> OnValidatedAsync(Order.ValidatedOrder order, CancellationToken cancellationToken)
    {
        return Task.FromResult<IOrder>(order);
    }

    protected virtual Task<IOrder> OnInvalidAsync(Order.InvalidOrder order, CancellationToken cancellationToken)
    {
        return Task.FromResult<IOrder>(order);
    }

    protected virtual Task<IOrder> OnPricedAsync(Order.PricedOrder order, CancellationToken cancellationToken)
    {
        return Task.FromResult<IOrder>(order);
    }

    protected virtual Task<IOrder> OnPersistedAsync(Order.PersistedOrder order, CancellationToken cancellationToken)
    {
        return Task.FromResult<IOrder>(order);
    }

    protected virtual Task<IOrder> OnPublishedAsync(Order.PublishedOrder order, CancellationToken cancellationToken)
    {
        return Task.FromResult<IOrder>(order);
    }

    protected override Task<IOrder> OnUnvalidatedAsync(Order.UnvalidatedOrder order, object state, CancellationToken cancellationToken)
    {
        return OnUnvalidatedAsync(order, cancellationToken);
    }

    protected override Task<IOrder> OnValidatedAsync(Order.ValidatedOrder order, object state, CancellationToken cancellationToken)
    {
        return OnValidatedAsync(order, cancellationToken);
    }

    protected override Task<IOrder> OnInvalidAsync(Order.InvalidOrder order, object state, CancellationToken cancellationToken)
    {
        return OnInvalidAsync(order, cancellationToken);
    }

    protected override Task<IOrder> OnPricedAsync(Order.PricedOrder order, object state, CancellationToken cancellationToken)
    {
        return OnPricedAsync(order, cancellationToken);
    }

    protected override Task<IOrder> OnPersistedAsync(Order.PersistedOrder order, object state, CancellationToken cancellationToken)
    {
        return OnPersistedAsync(order, cancellationToken);
    }

    protected override Task<IOrder> OnPublishedAsync(Order.PublishedOrder order, object state, CancellationToken cancellationToken)
    {
        return OnPublishedAsync(order, cancellationToken);
    }
}

