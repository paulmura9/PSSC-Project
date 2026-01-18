using static Shipment.Domain.Models.Shipment;

namespace Shipment.Domain.Operations;

/// <summary>
/// Shipment operation with a state dependency
/// NO VALIDATION - data is pre-validated by Ordering
/// </summary>
/// <typeparam name="TState">The state/dependency type for the operation</typeparam>
public abstract class ShipmentStateOperation<TState> : ShipmentOperationBase<TState>
{
    public override async Task<IShipment> TransformAsync(IShipment shipment, TState state, CancellationToken cancellationToken)
    {
        return shipment switch
        {
            CreatedShipment created => await OnCreatedAsync(created, state, cancellationToken),
            ScheduledShipment scheduled => await OnScheduledAsync(scheduled, state, cancellationToken),
            DispatchedShipment dispatched => await OnDispatchedAsync(dispatched, state, cancellationToken),
            DeliveredShipment delivered => await OnDeliveredAsync(delivered, state, cancellationToken),
            CancelledShipment cancelled => await OnCancelledAsync(cancelled, state, cancellationToken),
            ReturnedShipment returned => await OnReturnedAsync(returned, state, cancellationToken),
            PersistedShipment persisted => await OnPersistedAsync(persisted, state, cancellationToken),
            _ => throw new InvalidOperationException($"Unknown shipment state: {shipment.GetType().Name}")
        };
    }

    protected virtual Task<IShipment> OnCreatedAsync(CreatedShipment shipment, TState state, CancellationToken cancellationToken)
    {
        return Task.FromResult<IShipment>(shipment);
    }

    protected virtual Task<IShipment> OnScheduledAsync(ScheduledShipment shipment, TState state, CancellationToken cancellationToken)
    {
        return Task.FromResult<IShipment>(shipment);
    }

    protected virtual Task<IShipment> OnDispatchedAsync(DispatchedShipment shipment, TState state, CancellationToken cancellationToken)
    {
        return Task.FromResult<IShipment>(shipment);
    }

    protected virtual Task<IShipment> OnDeliveredAsync(DeliveredShipment shipment, TState state, CancellationToken cancellationToken)
    {
        return Task.FromResult<IShipment>(shipment);
    }

    protected virtual Task<IShipment> OnCancelledAsync(CancelledShipment shipment, TState state, CancellationToken cancellationToken)
    {
        return Task.FromResult<IShipment>(shipment);
    }

    protected virtual Task<IShipment> OnReturnedAsync(ReturnedShipment shipment, TState state, CancellationToken cancellationToken)
    {
        return Task.FromResult<IShipment>(shipment);
    }

    protected virtual Task<IShipment> OnPersistedAsync(PersistedShipment shipment, TState state, CancellationToken cancellationToken)
    {
        return Task.FromResult<IShipment>(shipment);
    }
}

