using Ordering.Domain.Exceptions;
using SharedKernel;
using static Ordering.Domain.Models.Order;

namespace Ordering.Domain.Operations;

/// <summary>
/// Order operation with a state dependency
/// </summary>
/// <typeparam name="TState">The state/dependency type for the operation</typeparam>
public abstract class OrderOperationWithState<TState> : DomainOperation<IOrder, TState, IOrder>
{
    public override async Task<IOrder> TransformAsync(IOrder order, TState state, CancellationToken cancellationToken)
    {
        return order switch
        {
            UnvalidatedOrder unvalidated => await OnUnvalidatedAsync(unvalidated, state, cancellationToken),
            PricedOrder priced => await OnPricedAsync(priced, state, cancellationToken),
            PersistableOrder persistable => await OnPersistableAsync(persistable, state, cancellationToken),
            PersistedOrder persisted => await OnPersistedAsync(persisted, state, cancellationToken),
            _ => throw new InvalidOrderStateException($"Unknown order state: {order.GetType().Name}")
        };
    }

    protected virtual Task<IOrder> OnUnvalidatedAsync(UnvalidatedOrder order, TState state, CancellationToken cancellationToken)
    {
        return Task.FromResult<IOrder>(order);
    }
    
    protected virtual Task<IOrder> OnPricedAsync(PricedOrder order, TState state, CancellationToken cancellationToken)
    {
        return Task.FromResult<IOrder>(order);
    }

    protected virtual Task<IOrder> OnPersistableAsync(PersistableOrder order, TState state, CancellationToken cancellationToken)
    {
        return Task.FromResult<IOrder>(order);
    }

    protected virtual Task<IOrder> OnPersistedAsync(PersistedOrder order, TState state, CancellationToken cancellationToken)
    {
        return Task.FromResult<IOrder>(order);
    }
    
}

