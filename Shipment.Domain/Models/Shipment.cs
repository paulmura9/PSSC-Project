using SharedKernel;
using SharedKernel.StateMachine;
using Shipment.Domain.Events;

namespace Shipment.Domain.Models;

/// <summary>
/// Shipment state enumeration for tracking current state
/// </summary>
public enum ShipmentState
{
    Created,                // Shipment created from order event
    ShippingCostCalculated, // Shipping cost calculated
    Scheduled,              // Scheduled for dispatch (tracking number assigned)
    Dispatched,             // Sent out for delivery
    Delivered,              // Successfully delivered
    Cancelled,              // Cancelled (from OrderStateChangedEvent with Status=Cancelled)
    Returned,               // Returned by customer
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

    /// <summary>
    /// Marker interface for all shipment states
    /// </summary>
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
    /// Represents a scheduled shipment with tracking number
    /// </summary>
    public record ScheduledShipment : IShipment
    {
        public ScheduledShipment(
            Guid shipmentId,
            Guid orderId,
            Guid userId,
            Money totalPrice,
            Money shippingCost,
            Money totalWithShipping,
            IReadOnlyCollection<ShipmentLine> lines,
            TrackingNumber trackingNumber,
            DateTime orderPlacedAt,
            DateTime scheduledAt)
        {
            ShipmentId = shipmentId;
            OrderId = orderId;
            UserId = userId;
            TotalPrice = totalPrice;
            ShippingCost = shippingCost;
            TotalWithShipping = totalWithShipping;
            Lines = lines;
            TrackingNumber = trackingNumber;
            OrderPlacedAt = orderPlacedAt;
            ScheduledAt = scheduledAt;
        }

        public Guid ShipmentId { get; }
        public Guid OrderId { get; }
        public Guid UserId { get; }
        public Money TotalPrice { get; }
        public Money ShippingCost { get; }
        public Money TotalWithShipping { get; }
        public IReadOnlyCollection<ShipmentLine> Lines { get; }
        public TrackingNumber TrackingNumber { get; }
        public DateTime OrderPlacedAt { get; }
        public DateTime ScheduledAt { get; }
    }

    /// <summary>
    /// Represents a dispatched shipment (sent out for delivery)
    /// </summary>
    public record DispatchedShipment : IShipment
    {
        public DispatchedShipment(
            Guid shipmentId,
            Guid orderId,
            Guid userId,
            Money totalPrice,
            Money shippingCost,
            Money totalWithShipping,
            IReadOnlyCollection<ShipmentLine> lines,
            TrackingNumber trackingNumber,
            DateTime orderPlacedAt,
            DateTime dispatchedAt)
        {
            ShipmentId = shipmentId;
            OrderId = orderId;
            UserId = userId;
            TotalPrice = totalPrice;
            ShippingCost = shippingCost;
            TotalWithShipping = totalWithShipping;
            Lines = lines;
            TrackingNumber = trackingNumber;
            OrderPlacedAt = orderPlacedAt;
            DispatchedAt = dispatchedAt;
        }

        public Guid ShipmentId { get; }
        public Guid OrderId { get; }
        public Guid UserId { get; }
        public Money TotalPrice { get; }
        public Money ShippingCost { get; }
        public Money TotalWithShipping { get; }
        public IReadOnlyCollection<ShipmentLine> Lines { get; }
        public TrackingNumber TrackingNumber { get; }
        public DateTime OrderPlacedAt { get; }
        public DateTime DispatchedAt { get; }
    }

    /// <summary>
    /// Represents a delivered shipment (successfully delivered)
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
    /// Represents a cancelled shipment
    /// </summary>
    public record CancelledShipment : IShipment
    {
        public CancelledShipment(
            Guid orderId,
            Guid userId,
            string reason,
            DateTime cancelledAt)
        {
            OrderId = orderId;
            UserId = userId;
            Reason = reason;
            CancelledAt = cancelledAt;
        }

        public Guid OrderId { get; }
        public Guid UserId { get; }
        public string Reason { get; }
        public DateTime CancelledAt { get; }
    }

    /// <summary>
    /// Represents a returned shipment
    /// </summary>
    public record ReturnedShipment : IShipment
    {
        public ReturnedShipment(
            Guid shipmentId,
            Guid orderId,
            Guid userId,
            TrackingNumber trackingNumber,
            string returnReason,
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
        public string ReturnReason { get; }
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
            Money shippingCost,
            Money totalWithShipping,
            IReadOnlyCollection<ShipmentLine> lines,
            TrackingNumber trackingNumber,
            DateTime orderPlacedAt,
            DateTime persistedAt)
        {
            ShipmentId = shipmentId;
            OrderId = orderId;
            UserId = userId;
            TotalPrice = totalPrice;
            ShippingCost = shippingCost;
            TotalWithShipping = totalWithShipping;
            Lines = lines;
            TrackingNumber = trackingNumber;
            OrderPlacedAt = orderPlacedAt;
            PersistedAt = persistedAt;
        }

        public Guid ShipmentId { get; }
        public Guid OrderId { get; }
        public Guid UserId { get; }
        public Money TotalPrice { get; }
        public Money ShippingCost { get; }
        public Money TotalWithShipping { get; }
        public IReadOnlyCollection<ShipmentLine> Lines { get; }
        public TrackingNumber TrackingNumber { get; }
        public DateTime OrderPlacedAt { get; }
        public DateTime PersistedAt { get; }
    }

    /// <summary>
    /// Extension method to convert shipment state to event (Lab-style pattern)
    /// </summary>
    public static IShipmentSentEvent ToEvent(this IShipment shipment) => shipment switch
    {
        CreatedShipment _ => new ShipmentSendFailedEvent 
        { 
            Reasons = new[] { "Unexpected created state" } 
        },
        ShippingCostCalculatedShipment _ => new ShipmentSendFailedEvent 
        { 
            Reasons = new[] { "Unexpected shipping cost calculated state" } 
        },
        ScheduledShipment _ => new ShipmentSendFailedEvent 
        { 
            Reasons = new[] { "Unexpected scheduled state" } 
        },
        DispatchedShipment _ => new ShipmentSendFailedEvent 
        { 
            Reasons = new[] { "Unexpected dispatched state" } 
        },
        PersistedShipment persisted => new ShipmentSentEvent
        {
            ShipmentId = persisted.ShipmentId,
            OrderId = persisted.OrderId,
            UserId = persisted.UserId,
            TrackingNumber = persisted.TrackingNumber.Value,
            TotalPrice = persisted.TotalWithShipping.Value,
            SentAt = DateTime.UtcNow
        },
        CancelledShipment cancelled => new ShipmentSendFailedEvent 
        { 
            OrderId = cancelled.OrderId,
            Reasons = new[] { cancelled.Reason } 
        },
        ReturnedShipment returned => new ShipmentSendFailedEvent 
        { 
            OrderId = returned.OrderId,
            Reasons = new[] { returned.ReturnReason } 
        },
        DeliveredShipment _ => new ShipmentSendFailedEvent 
        { 
            Reasons = new[] { "Unexpected delivered state" } 
        },
        _ => throw new NotImplementedException($"Unknown shipment state: {shipment.GetType().Name}")
    };
}
