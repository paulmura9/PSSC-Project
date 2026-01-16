namespace Shipment.Domain.Models;

/// <summary>
/// Static class containing all shipment state records (DDD pattern)
/// </summary>
public static class Shipment
{
    public interface IShipment { }

    /// <summary>
    /// Represents an unprocessed shipment from order event
    /// </summary>
    public record UnprocessedShipment : IShipment
    {
        public UnprocessedShipment(
            Guid orderId,
            Guid userId,
            decimal totalPrice,
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
        public decimal TotalPrice { get; }
        public IReadOnlyCollection<ShipmentLine> Lines { get; }
        public DateTime OrderPlacedAt { get; }
    }

    /// <summary>
    /// Represents a shipment that failed validation
    /// </summary>
    public record InvalidShipment : IShipment
    {
        public InvalidShipment(Guid orderId, IEnumerable<string> reasons)
        {
            OrderId = orderId;
            Reasons = reasons;
        }

        public Guid OrderId { get; }
        public IEnumerable<string> Reasons { get; }
    }

    /// <summary>
    /// Represents a validated shipment
    /// </summary>
    public record ValidatedShipment : IShipment
    {
        public ValidatedShipment(
            Guid orderId,
            Guid userId,
            decimal totalPrice,
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
        public decimal TotalPrice { get; }
        public IReadOnlyCollection<ShipmentLine> Lines { get; }
        public DateTime OrderPlacedAt { get; }
    }

    /// <summary>
    /// Represents a processed shipment ready for shipping
    /// </summary>
    public record ProcessedShipment : IShipment
    {
        public ProcessedShipment(
            Guid shipmentId,
            Guid orderId,
            Guid userId,
            decimal totalPrice,
            IReadOnlyCollection<ShipmentLine> lines,
            string trackingNumber,
            DateTime orderPlacedAt)
        {
            ShipmentId = shipmentId;
            OrderId = orderId;
            UserId = userId;
            TotalPrice = totalPrice;
            Lines = lines;
            TrackingNumber = trackingNumber;
            OrderPlacedAt = orderPlacedAt;
        }

        public Guid ShipmentId { get; }
        public Guid OrderId { get; }
        public Guid UserId { get; }
        public decimal TotalPrice { get; }
        public IReadOnlyCollection<ShipmentLine> Lines { get; }
        public string TrackingNumber { get; }
        public DateTime OrderPlacedAt { get; }
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
            decimal totalPrice,
            IReadOnlyCollection<ShipmentLine> lines,
            string trackingNumber,
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
        public decimal TotalPrice { get; }
        public IReadOnlyCollection<ShipmentLine> Lines { get; }
        public string TrackingNumber { get; }
        public DateTime OrderPlacedAt { get; }
        public DateTime PersistedAt { get; }
    }
}

