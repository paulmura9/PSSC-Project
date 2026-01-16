using static Shipment.Domain.Models.Shipment;

namespace Shipment.Domain.Operations;

/// <summary>
/// Base class for domain operations following the DDD pattern for Shipment
/// </summary>
/// <typeparam name="TState">The state/dependency type for the operation</typeparam>
public abstract class ShipmentOperation<TState>
{
    public abstract Task<IShipment> TransformAsync(IShipment shipment, TState state, CancellationToken cancellationToken);
}

/// <summary>
/// Shipment operation with a state dependency
/// </summary>
/// <typeparam name="TState">The state/dependency type for the operation</typeparam>
public abstract class ShipmentStateOperation<TState> : ShipmentOperation<TState>
{
    public override async Task<IShipment> TransformAsync(IShipment shipment, TState state, CancellationToken cancellationToken)
    {
        return shipment switch
        {
            UnprocessedShipment unprocessed => await OnUnprocessedAsync(unprocessed, state, cancellationToken),
            ValidatedShipment validated => await OnValidatedAsync(validated, state, cancellationToken),
            InvalidShipment invalid => await OnInvalidAsync(invalid, state, cancellationToken),
            ProcessedShipment processed => await OnProcessedAsync(processed, state, cancellationToken),
            PersistedShipment persisted => await OnPersistedAsync(persisted, state, cancellationToken),
            _ => throw new InvalidOperationException($"Unknown shipment state: {shipment.GetType().Name}")
        };
    }

    protected virtual Task<IShipment> OnUnprocessedAsync(UnprocessedShipment shipment, TState state, CancellationToken cancellationToken)
    {
        return Task.FromResult<IShipment>(shipment);
    }

    protected virtual Task<IShipment> OnValidatedAsync(ValidatedShipment shipment, TState state, CancellationToken cancellationToken)
    {
        return Task.FromResult<IShipment>(shipment);
    }

    protected virtual Task<IShipment> OnInvalidAsync(InvalidShipment shipment, TState state, CancellationToken cancellationToken)
    {
        return Task.FromResult<IShipment>(shipment);
    }

    protected virtual Task<IShipment> OnProcessedAsync(ProcessedShipment shipment, TState state, CancellationToken cancellationToken)
    {
        return Task.FromResult<IShipment>(shipment);
    }

    protected virtual Task<IShipment> OnPersistedAsync(PersistedShipment shipment, TState state, CancellationToken cancellationToken)
    {
        return Task.FromResult<IShipment>(shipment);
    }
}

/// <summary>
/// Shipment operation without state dependency
/// </summary>
public abstract class ShipmentOperation : ShipmentStateOperation<object>
{
    public Task<IShipment> TransformAsync(IShipment shipment, CancellationToken cancellationToken)
    {
        return TransformAsync(shipment, null!, cancellationToken);
    }
}

