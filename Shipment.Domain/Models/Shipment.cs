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
    Dispatched,             // Sent out for delivery (premium)
    Persisted,              // Saved to database
    Published               // Published to Service Bus
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
    /// - Created → ShippingCostCalculated (calculate shipping)
    /// - ShippingCostCalculated → Scheduled (assign tracking)
    /// - Scheduled → Dispatched (premium only) or Persisted (regular)
    /// - Dispatched → Persisted
    /// - Persisted → Published
    /// </summary>
    public static readonly StateTransitionMap<ShipmentState> Transitions = new StateTransitionMap<ShipmentState>()
        .Allow(ShipmentState.Created, ShipmentState.ShippingCostCalculated)
        .Allow(ShipmentState.ShippingCostCalculated, ShipmentState.Scheduled)
        .Allow(ShipmentState.Scheduled, ShipmentState.Dispatched, ShipmentState.Persisted)
        .Allow(ShipmentState.Dispatched, ShipmentState.Persisted)
        .Allow(ShipmentState.Persisted, ShipmentState.Published);

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
            PersistedShipment => ShipmentState.Persisted,
            PublishedShipment => ShipmentState.Published,
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
            bool premiumSubscription,
            Money totalPrice,
            IReadOnlyCollection<ShipmentLine> lines,
            DateTime orderPlacedAt)
        {
            OrderId = orderId;
            UserId = userId;
            PremiumSubscription = premiumSubscription;
            TotalPrice = totalPrice;
            Lines = lines;
            OrderPlacedAt = orderPlacedAt;
        }

        public Guid OrderId { get; }
        public Guid UserId { get; }
        public bool PremiumSubscription { get; }
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
    /// Represents a shipment that has been published to Service Bus
    /// </summary>
    public record PublishedShipment : IShipment
    {
        public PublishedShipment(PersistedShipment persisted, DateTime publishedAt)
        {
            ShipmentId = persisted.ShipmentId;
            OrderId = persisted.OrderId;
            UserId = persisted.UserId;
            TotalPrice = persisted.TotalPrice;
            ShippingCost = persisted.ShippingCost;
            TotalWithShipping = persisted.TotalWithShipping;
            Lines = persisted.Lines;
            TrackingNumber = persisted.TrackingNumber;
            OrderPlacedAt = persisted.OrderPlacedAt;
            PublishedAt = publishedAt;
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
        public DateTime PublishedAt { get; }
    }

    /// <summary>
    /// Extension method to convert shipment state to event (Lab-style pattern)
    /// </summary>
    public static IShipmentWorkflowResult ToEvent(this IShipment shipment) => shipment switch
    {
        PublishedShipment published => new ShipmentCreatedSuccessEvent
        {
            ShipmentId = published.ShipmentId,
            OrderId = published.OrderId,
            UserId = published.UserId,
            TrackingNumber = published.TrackingNumber.Value,
            TotalPrice = published.TotalWithShipping.Value,
            CreatedAt = published.PublishedAt
        },
        PersistedShipment persisted => new ShipmentCreatedSuccessEvent
        {
            ShipmentId = persisted.ShipmentId,
            OrderId = persisted.OrderId,
            UserId = persisted.UserId,
            TrackingNumber = persisted.TrackingNumber.Value,
            TotalPrice = persisted.TotalWithShipping.Value,
            CreatedAt = DateTime.UtcNow
        },
        _ => new ShipmentCreatedFailedEvent
        { 
            Reasons = new[] { $"Unexpected shipment state: {shipment.GetType().Name}" } 
        }
    };
}
