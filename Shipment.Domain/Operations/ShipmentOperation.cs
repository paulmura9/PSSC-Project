using static Shipment.Domain.Models.Shipment;

namespace Shipment.Domain.Operations;

/// <summary>
/// Shipment operation without state dependency
/// NO VALIDATION - data is pre-validated by Ordering
/// </summary>
public abstract class ShipmentOperation : ShipmentStateOperation<object>
{
    public Task<IShipment> TransformAsync(IShipment shipment, CancellationToken cancellationToken)
    {
        return TransformAsync(shipment, null!, cancellationToken);
    }

    // Virtual methods that delegate to versions without state parameter
    protected virtual Task<IShipment> OnCreatedAsync(CreatedShipment shipment, CancellationToken cancellationToken)
        => Task.FromResult<IShipment>(shipment);

    protected virtual Task<IShipment> OnScheduledAsync(ScheduledShipment shipment, CancellationToken cancellationToken)
        => Task.FromResult<IShipment>(shipment);

    protected virtual Task<IShipment> OnDispatchedAsync(DispatchedShipment shipment, CancellationToken cancellationToken)
        => Task.FromResult<IShipment>(shipment);

    protected virtual Task<IShipment> OnDeliveredAsync(DeliveredShipment shipment, CancellationToken cancellationToken)
        => Task.FromResult<IShipment>(shipment);

    protected virtual Task<IShipment> OnCancelledAsync(CancelledShipment shipment, CancellationToken cancellationToken)
        => Task.FromResult<IShipment>(shipment);

    protected virtual Task<IShipment> OnReturnedAsync(ReturnedShipment shipment, CancellationToken cancellationToken)
        => Task.FromResult<IShipment>(shipment);

    protected virtual Task<IShipment> OnPersistedAsync(PersistedShipment shipment, CancellationToken cancellationToken)
        => Task.FromResult<IShipment>(shipment);

    // Override base class methods to delegate to stateless versions
    protected override Task<IShipment> OnCreatedAsync(CreatedShipment shipment, object state, CancellationToken cancellationToken)
        => OnCreatedAsync(shipment, cancellationToken);


    protected override Task<IShipment> OnScheduledAsync(ScheduledShipment shipment, object state, CancellationToken cancellationToken)
        => OnScheduledAsync(shipment, cancellationToken);

    protected override Task<IShipment> OnDispatchedAsync(DispatchedShipment shipment, object state, CancellationToken cancellationToken)
        => OnDispatchedAsync(shipment, cancellationToken);

    protected override Task<IShipment> OnDeliveredAsync(DeliveredShipment shipment, object state, CancellationToken cancellationToken)
        => OnDeliveredAsync(shipment, cancellationToken);

    protected override Task<IShipment> OnCancelledAsync(CancelledShipment shipment, object state, CancellationToken cancellationToken)
        => OnCancelledAsync(shipment, cancellationToken);

    protected override Task<IShipment> OnReturnedAsync(ReturnedShipment shipment, object state, CancellationToken cancellationToken)
        => OnReturnedAsync(shipment, cancellationToken);

    protected override Task<IShipment> OnPersistedAsync(PersistedShipment shipment, object state, CancellationToken cancellationToken)
        => OnPersistedAsync(shipment, cancellationToken);
}
