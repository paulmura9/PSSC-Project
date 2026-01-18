using static Ordering.Domain.Models.Order;

namespace Ordering.Domain.Operations;

/// <summary>
/// Order operation without state dependency
/// </summary>
public abstract class OrderOperation : OrderOperationWithState<object>
{
    public Task<IOrder> TransformAsync(IOrder order, CancellationToken cancellationToken)
    {
        return TransformAsync(order, null!, cancellationToken);
    }

    protected virtual Task<IOrder> OnUnvalidatedAsync(UnvalidatedOrder order, CancellationToken cancellationToken)
    {
        return Task.FromResult<IOrder>(order);
    }

    protected virtual Task<IOrder> OnValidatedAsync(ValidatedOrder order, CancellationToken cancellationToken)
    {
        return Task.FromResult<IOrder>(order);
    }

    protected virtual Task<IOrder> OnInvalidAsync(InvalidOrder order, CancellationToken cancellationToken)
    {
        return Task.FromResult<IOrder>(order);
    }

    protected virtual Task<IOrder> OnPricedAsync(PricedOrder order, CancellationToken cancellationToken)
    {
        return Task.FromResult<IOrder>(order);
    }

    protected virtual Task<IOrder> OnPersistableAsync(PersistableOrder order, CancellationToken cancellationToken)
    {
        return Task.FromResult<IOrder>(order);
    }

    protected virtual Task<IOrder> OnPersistedAsync(PersistedOrder order, CancellationToken cancellationToken)
    {
        return Task.FromResult<IOrder>(order);
    }

    protected virtual Task<IOrder> OnPublishedAsync(PublishedOrder order, CancellationToken cancellationToken)
    {
        return Task.FromResult<IOrder>(order);
    }

    protected override Task<IOrder> OnUnvalidatedAsync(UnvalidatedOrder order, object state, CancellationToken cancellationToken)
    {
        return OnUnvalidatedAsync(order, cancellationToken);
    }

    protected override Task<IOrder> OnValidatedAsync(ValidatedOrder order, object state, CancellationToken cancellationToken)
    {
        return OnValidatedAsync(order, cancellationToken);
    }

    protected override Task<IOrder> OnInvalidAsync(InvalidOrder order, object state, CancellationToken cancellationToken)
    {
        return OnInvalidAsync(order, cancellationToken);
    }

    protected override Task<IOrder> OnPricedAsync(PricedOrder order, object state, CancellationToken cancellationToken)
    {
        return OnPricedAsync(order, cancellationToken);
    }

    protected override Task<IOrder> OnPersistableAsync(PersistableOrder order, object state, CancellationToken cancellationToken)
    {
        return OnPersistableAsync(order, cancellationToken);
    }

    protected override Task<IOrder> OnPersistedAsync(PersistedOrder order, object state, CancellationToken cancellationToken)
    {
        return OnPersistedAsync(order, cancellationToken);
    }

    protected override Task<IOrder> OnPublishedAsync(PublishedOrder order, object state, CancellationToken cancellationToken)
    {
        return OnPublishedAsync(order, cancellationToken);
    }
}

