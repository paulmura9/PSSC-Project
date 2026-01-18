using SharedKernel;

namespace Shipment.Domain.Models;

/// <summary>
/// Shipment state enumeration for tracking current state
/// NO VALIDATION - data is pre-validated by Ordering
/// </summary>
public enum ShipmentState
{
    Created,                // Shipment created from order event
    ShippingCostCalculated, // Shipping cost calculated (0 for premium, 30-100 RON for regular based on order total)
    Scheduled,              // Scheduled for dispatch (tracking number assigned)
    Dispatched,             // Sent out for delivery
    Delivered,              // Successfully delivered
    Cancelled,              // Cancelled (from OrderStateChangedEvent with Status=Cancelled)
    Returned,               // Returned by customer (from OrderStateChangedEvent with Status=Returned)
    Persisted               // Saved to database (internal state)
}

/// <summary>
/// Static class containing all shipment state records (DDD pattern)
/// Shipment does NOT validate - Ordering already validated the data
/// </summary>
public static class Shipment
{
    /// <summary>
    /// Defines allowed state transitions for Shipment
    /// Business rules:
    /// - Created → ShippingCostCalculated (calculate shipping) or Cancelled
    /// - ShippingCostCalculated → Scheduled (assign tracking) or Cancelled
    /// - Scheduled → Dispatched (sent out) or Cancelled
    /// - Dispatched → Delivered (successful) or Returned (customer returned)
    /// - Persisted → Scheduled (continue processing)
    /// </summary>
    public static readonly StateTransitionMap<ShipmentState> Transitions = new StateTransitionMap<ShipmentState>()
        .Allow(ShipmentState.Created, ShipmentState.ShippingCostCalculated, ShipmentState.Cancelled, ShipmentState.Persisted)
        .Allow(ShipmentState.ShippingCostCalculated, ShipmentState.Scheduled, ShipmentState.Cancelled)
        .Allow(ShipmentState.Scheduled, ShipmentState.Dispatched, ShipmentState.Cancelled)
        .Allow(ShipmentState.Dispatched, ShipmentState.Delivered, ShipmentState.Returned)
        .Allow(ShipmentState.Persisted, ShipmentState.Scheduled, ShipmentState.Cancelled);

    public interface IShipment : IStateMachine<ShipmentState>
    {
        ShipmentState IStateMachine<ShipmentState>.CurrentState => this switch
        {
            CreatedShipment => ShipmentState.Created,
            ShippingCostCalculatedShipment => ShipmentState.ShippingCostCalculated,
            ScheduledShipment => ShipmentState.Scheduled,
            DispatchedShipment => ShipmentState.Dispatched,
            DeliveredShipment => ShipmentState.Delivered,
            CancelledShipment => ShipmentState.Cancelled,
            ReturnedShipment => ShipmentState.Returned,
            PersistedShipment => ShipmentState.Persisted,
            _ => throw new InvalidOperationException($"Unknown shipment state: {GetType().Name}")
        };

        bool IStateMachine<ShipmentState>.CanTransitionTo(ShipmentState targetState) =>
            Transitions.IsAllowed(((IStateMachine<ShipmentState>)this).CurrentState, targetState);
    }

    /// <summary>
    /// Represents a newly created shipment from order event
    /// Data is already validated by Ordering
    /// </summary>
    public record CreatedShipment : IShipment
    {
        public CreatedShipment(
            Guid orderId,
            Guid userId,
            Money totalPrice,
            IReadOnlyCollection<ShipmentLine> lines,
            DateTime orderPlacedAt)
        {
            OrderId = orderId;
            UserId = userId;
            TotalPrice = totalPrice;
            Lines = lines;
            OrderPlacedAt = orderPlacedAt;
        }

        public Guid OrderId { get; }
        public Guid UserId { get; }
        public Money TotalPrice { get; }
        public IReadOnlyCollection<ShipmentLine> Lines { get; }
        public DateTime OrderPlacedAt { get; }
    }

    /// <summary>
    /// Represents a shipment with calculated shipping cost
    /// Premium subscribers get free shipping
    /// Regular customers pay based on order total (in RON):
    /// - 0-3000 RON: 30 RON
    /// - 3001-6000 RON: 50 RON
    /// - 6001-10000 RON: 75 RON
    /// - >= 10001 RON: 100 RON
    /// </summary>
    public record ShippingCostCalculatedShipment : IShipment
    {
        public ShippingCostCalculatedShipment(
            Guid orderId,
            Guid userId,
            bool premiumSubscription,
            Money orderTotal,
            Money shippingCost,
            Money totalWithShipping,
            IReadOnlyCollection<ShipmentLine> lines,
            DateTime orderPlacedAt,
            DateTime calculatedAt)
        {
            OrderId = orderId;
            UserId = userId;
            PremiumSubscription = premiumSubscription;
            OrderTotal = orderTotal;
            ShippingCost = shippingCost;
            TotalWithShipping = totalWithShipping;
            Lines = lines;
            OrderPlacedAt = orderPlacedAt;
            CalculatedAt = calculatedAt;
        }

        public Guid OrderId { get; }
        public Guid UserId { get; }
        public bool PremiumSubscription { get; }
        public Money OrderTotal { get; }
        public Money ShippingCost { get; }
        public Money TotalWithShipping { get; }
        public IReadOnlyCollection<ShipmentLine> Lines { get; }
        public DateTime OrderPlacedAt { get; }
        public DateTime CalculatedAt { get; }
    }

    /// <summary>
    /// Represents a shipment scheduled for dispatch
    /// </summary>
    public record ScheduledShipment : IShipment
    {
        public ScheduledShipment(
            Guid shipmentId,
            Guid orderId,
            Guid userId,
            Money totalPrice,
            IReadOnlyCollection<ShipmentLine> lines,
            TrackingNumber trackingNumber,
            DateTime orderPlacedAt,
            DateTime scheduledAt)
        {
            ShipmentId = shipmentId;
            OrderId = orderId;
            UserId = userId;
            TotalPrice = totalPrice;
            Lines = lines;
            TrackingNumber = trackingNumber;
            OrderPlacedAt = orderPlacedAt;
            ScheduledAt = scheduledAt;
        }

        public Guid ShipmentId { get; }
        public Guid OrderId { get; }
        public Guid UserId { get; }
        public Money TotalPrice { get; }
        public IReadOnlyCollection<ShipmentLine> Lines { get; }
        public TrackingNumber TrackingNumber { get; }
        public DateTime OrderPlacedAt { get; }
        public DateTime ScheduledAt { get; }
    }

    /// <summary>
    /// Represents a shipment that has been dispatched (sent out for delivery)
    /// </summary>
    public record DispatchedShipment : IShipment
    {
        public DispatchedShipment(
            Guid shipmentId,
            Guid orderId,
            Guid userId,
            Money totalPrice,
            IReadOnlyCollection<ShipmentLine> lines,
            TrackingNumber trackingNumber,
            DateTime dispatchedAt)
        {
            ShipmentId = shipmentId;
            OrderId = orderId;
            UserId = userId;
            TotalPrice = totalPrice;
            Lines = lines;
            TrackingNumber = trackingNumber;
            DispatchedAt = dispatchedAt;
        }

        public Guid ShipmentId { get; }
        public Guid OrderId { get; }
        public Guid UserId { get; }
        public Money TotalPrice { get; }
        public IReadOnlyCollection<ShipmentLine> Lines { get; }
        public TrackingNumber TrackingNumber { get; }
        public DateTime DispatchedAt { get; }
    }

    /// <summary>
    /// Represents a shipment that has been successfully delivered
    /// </summary>
    public record DeliveredShipment : IShipment
    {
        public DeliveredShipment(
            Guid shipmentId,
            Guid orderId,
            Guid userId,
            TrackingNumber trackingNumber,
            DateTime deliveredAt)
        {
            ShipmentId = shipmentId;
            OrderId = orderId;
            UserId = userId;
            TrackingNumber = trackingNumber;
            DeliveredAt = deliveredAt;
        }

        public Guid ShipmentId { get; }
        public Guid OrderId { get; }
        public Guid UserId { get; }
        public TrackingNumber TrackingNumber { get; }
        public DateTime DeliveredAt { get; }
    }

    /// <summary>
    /// Represents a shipment that has been cancelled
    /// Triggered by OrderStateChangedEvent with Status=Cancelled
    /// </summary>
    public record CancelledShipment : IShipment
    {
        public CancelledShipment(
            Guid shipmentId,
            Guid orderId,
            CancellationReason reason,
            DateTime cancelledAt)
        {
            ShipmentId = shipmentId;
            OrderId = orderId;
            Reason = reason;
            CancelledAt = cancelledAt;
        }

        public Guid ShipmentId { get; }
        public Guid OrderId { get; }
        public CancellationReason Reason { get; }
        public DateTime CancelledAt { get; }
    }

    /// <summary>
    /// Represents a shipment that has been returned by customer
    /// Triggered by OrderStateChangedEvent with Status=Returned
    /// </summary>
    public record ReturnedShipment : IShipment
    {
        public ReturnedShipment(
            Guid shipmentId,
            Guid orderId,
            Guid userId,
            TrackingNumber trackingNumber,
            ReturnReason returnReason,
            DateTime returnedAt)
        {
            ShipmentId = shipmentId;
            OrderId = orderId;
            UserId = userId;
            TrackingNumber = trackingNumber;
            ReturnReason = returnReason;
            ReturnedAt = returnedAt;
        }

        public Guid ShipmentId { get; }
        public Guid OrderId { get; }
        public Guid UserId { get; }
        public TrackingNumber TrackingNumber { get; }
        public ReturnReason ReturnReason { get; }
        public DateTime ReturnedAt { get; }
    }

    /// <summary>
    /// Represents a shipment that has been persisted to the database
    /// </summary>
    public record PersistedShipment : IShipment
    {
        public PersistedShipment(
            Guid shipmentId,
            Guid orderId,
            Guid userId,
            Money totalPrice,
            IReadOnlyCollection<ShipmentLine> lines,
            TrackingNumber trackingNumber,
            DateTime orderPlacedAt,
            DateTime persistedAt)
        {
            ShipmentId = shipmentId;
            OrderId = orderId;
            UserId = userId;
            TotalPrice = totalPrice;
            Lines = lines;
            TrackingNumber = trackingNumber;
            OrderPlacedAt = orderPlacedAt;
            PersistedAt = persistedAt;
        }

        public Guid ShipmentId { get; }
        public Guid OrderId { get; }
        public Guid UserId { get; }
        public Money TotalPrice { get; }
        public IReadOnlyCollection<ShipmentLine> Lines { get; }
        public TrackingNumber TrackingNumber { get; }
        public DateTime OrderPlacedAt { get; }
        public DateTime PersistedAt { get; }
    }
}

