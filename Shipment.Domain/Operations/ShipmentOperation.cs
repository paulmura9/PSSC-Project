using SharedKernel;
using Shipment.Domain.Exceptions;
using static Shipment.Domain.Models.Shipment;

namespace Shipment.Domain.Operations;

/// <summary>
/// Base class for shipment operations with state dependency (SYNC - pure transformations)
/// Extends DomainOperation from SharedKernel
/// </summary>
/// <typeparam name="TState">The state/dependency type for the operation</typeparam>
public abstract class ShipmentOperation<TState> : DomainOperation<IShipment, TState, IShipment>
    where TState : class
{
    public override IShipment Transform(IShipment shipment, TState? state) => shipment switch
    {
        CreatedShipment created => OnCreated(created, state),
        ShippingCostCalculatedShipment calculated => OnShippingCostCalculated(calculated, state),
        ScheduledShipment scheduled => OnScheduled(scheduled, state),
        _ => throw new InvalidShipmentStateException(shipment.GetType().Name)
    };

    protected virtual IShipment OnCreated(CreatedShipment shipment, TState? state) => shipment;
    protected virtual IShipment OnShippingCostCalculated(ShippingCostCalculatedShipment shipment, TState? state) => shipment;
    protected virtual IShipment OnScheduled(ScheduledShipment shipment, TState? state) => shipment;
}

/// <summary>
/// Base class for shipment operations without state dependency (SYNC - pure transformations)
/// </summary>
public abstract class ShipmentOperation : ShipmentOperation<object>
{
    public IShipment Transform(IShipment shipment) => Transform(shipment, null);

    protected sealed override IShipment OnCreated(CreatedShipment shipment, object? state) => OnCreated(shipment);
    protected virtual IShipment OnCreated(CreatedShipment shipment) => shipment;

    protected sealed override IShipment OnShippingCostCalculated(ShippingCostCalculatedShipment shipment, object? state) => OnShippingCostCalculated(shipment);
    protected virtual IShipment OnShippingCostCalculated(ShippingCostCalculatedShipment shipment) => shipment;

    protected sealed override IShipment OnScheduled(ScheduledShipment shipment, object? state) => OnScheduled(shipment);
    protected virtual IShipment OnScheduled(ScheduledShipment shipment) => shipment;
}
