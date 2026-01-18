namespace Shipment.Domain.Operations;

/// <summary>
/// Base class for domain operations following the DDD pattern for Shipment
/// </summary>
/// <typeparam name="TState">The state/dependency type for the operation</typeparam>
public abstract class ShipmentOperationBase<TState>
{
    public abstract Task<Models.Shipment.IShipment> TransformAsync(Models.Shipment.IShipment shipment, TState state, CancellationToken cancellationToken);
}

