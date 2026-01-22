using Ordering.Domain.Exceptions;
using SharedKernel;
using static Ordering.Domain.Models.Order;

namespace Ordering.Domain.Operations;

/// <summary>
/// Base class for order operations with state dependency (SYNC - pure transformations)
/// Extends DomainOperation from SharedKernel
/// </summary>
/// <typeparam name="TState">The state/dependency type for the operation</typeparam>
public abstract class OrderOperation<TState> : DomainOperation<IOrder, TState, IOrder>
    where TState : class
{
    public override IOrder Transform(IOrder order, TState? state) => order switch
    {
        UnvalidatedOrder unvalidated => OnUnvalidated(unvalidated, state),
        PricedOrder priced => OnPriced(priced, state),
        _ => throw new InvalidOrderStateException($"Unknown order state: {order.GetType().Name}")
    };

    protected virtual IOrder OnUnvalidated(UnvalidatedOrder order, TState? state) => order;
    protected virtual IOrder OnPriced(PricedOrder order, TState? state) => order;
}

/// <summary>
/// Base class for order operations without state dependency (SYNC - pure transformations)
/// </summary>
public abstract class OrderOperation : OrderOperation<object>
{
    public IOrder Transform(IOrder order) => Transform(order, null);

    protected sealed override IOrder OnUnvalidated(UnvalidatedOrder order, object? state) => OnUnvalidated(order);
    protected virtual IOrder OnUnvalidated(UnvalidatedOrder order) => order;

    protected sealed override IOrder OnPriced(PricedOrder order, object? state) => OnPriced(order);
    protected virtual IOrder OnPriced(PricedOrder order) => order;
}
