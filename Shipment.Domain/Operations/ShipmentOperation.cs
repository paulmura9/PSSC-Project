using static Shipment.Domain.Models.Shipment;

namespace Shipment.Domain.Operations;

/// <summary>
/// Base class for shipment operations following Lab-style DDD pattern (SYNC - pure transformations)
/// Async is only used for DB persistence and Service Bus publishing in workflow
/// </summary>
public abstract class ShipmentOperation : ShipmentOperationWithState<object>
{
    public IShipment Transform(IShipment shipment)
    {
        return Transform(shipment, null!);
    }

    protected virtual IShipment OnCreated(CreatedShipment shipment) => shipment;
    protected virtual IShipment OnShippingCostCalculated(ShippingCostCalculatedShipment shipment) => shipment;
    protected virtual IShipment OnScheduled(ScheduledShipment shipment) => shipment;
    protected virtual IShipment OnDispatched(DispatchedShipment shipment) => shipment;
    protected virtual IShipment OnPersisted(PersistedShipment shipment) => shipment;
    protected virtual IShipment OnCancelled(CancelledShipment shipment) => shipment;
    protected virtual IShipment OnReturned(ReturnedShipment shipment) => shipment;

    protected override IShipment OnCreated(CreatedShipment shipment, object? state) => OnCreated(shipment);
    protected override IShipment OnShippingCostCalculated(ShippingCostCalculatedShipment shipment, object? state) => OnShippingCostCalculated(shipment);
    protected override IShipment OnScheduled(ScheduledShipment shipment, object? state) => OnScheduled(shipment);
    protected override IShipment OnDispatched(DispatchedShipment shipment, object? state) => OnDispatched(shipment);
    protected override IShipment OnPersisted(PersistedShipment shipment, object? state) => OnPersisted(shipment);
    protected override IShipment OnCancelled(CancelledShipment shipment, object? state) => OnCancelled(shipment);
    protected override IShipment OnReturned(ReturnedShipment shipment, object? state) => OnReturned(shipment);
}

/// <summary>
/// Base class for shipment operations that need external state/dependencies (SYNC - pure transformations)
/// </summary>
public abstract class ShipmentOperationWithState<TState> where TState : class
{
    public IShipment Transform(IShipment shipment, TState? state)
    {
        return shipment switch
        {
            CreatedShipment created => OnCreated(created, state),
            ShippingCostCalculatedShipment calculated => OnShippingCostCalculated(calculated, state),
            ScheduledShipment scheduled => OnScheduled(scheduled, state),
            DispatchedShipment dispatched => OnDispatched(dispatched, state),
            PersistedShipment persisted => OnPersisted(persisted, state),
            CancelledShipment cancelled => OnCancelled(cancelled, state),
            ReturnedShipment returned => OnReturned(returned, state),
            _ => shipment
        };
    }

    protected virtual IShipment OnCreated(CreatedShipment shipment, TState? state) => shipment;
    protected virtual IShipment OnShippingCostCalculated(ShippingCostCalculatedShipment shipment, TState? state) => shipment;
    protected virtual IShipment OnScheduled(ScheduledShipment shipment, TState? state) => shipment;
    protected virtual IShipment OnDispatched(DispatchedShipment shipment, TState? state) => shipment;
    protected virtual IShipment OnPersisted(PersistedShipment shipment, TState? state) => shipment;
    protected virtual IShipment OnCancelled(CancelledShipment shipment, TState? state) => shipment;
    protected virtual IShipment OnReturned(ReturnedShipment shipment, TState? state) => shipment;
}

