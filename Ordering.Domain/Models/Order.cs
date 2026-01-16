namespace Ordering.Domain.Models;

/// <summary>
/// Static class containing all order state records (similar to Exam.cs in lab)
/// </summary>
public static class Order
{
    /// <summary>
    /// Marker interface for all order states
    /// </summary>
    public interface IOrder { }

    /// <summary>
    /// Represents an unvalidated order from user input
    /// </summary>
    public record UnvalidatedOrder : IOrder
    {
        public UnvalidatedOrder(IReadOnlyCollection<UnvalidatedOrderLine> lines,
            Guid userId,
            string deliveryAddress,
            string postalCode,
            string phone,
            string cardNumber,
            string cvv,
            string expiry)
        {
            Lines = lines;
            UserId = userId;
            DeliveryAddress = deliveryAddress;
            PostalCode = postalCode;
            Phone = phone;
            CardNumber = cardNumber;
            Cvv = cvv;
            Expiry = expiry;
        }

        public IReadOnlyCollection<UnvalidatedOrderLine> Lines { get; }
        public Guid UserId { get; }
        public string DeliveryAddress { get; }
        public string PostalCode { get; }
        public string Phone { get; }
        public string CardNumber { get; }
        public string Cvv { get; }
        public string Expiry { get; }
    }

    /// <summary>
    /// Represents an invalid order with validation errors
    /// </summary>
    public record InvalidOrder : IOrder
    {
        internal InvalidOrder(IReadOnlyCollection<UnvalidatedOrderLine> lines, IEnumerable<string> reasons)
        {
            Lines = lines;
            Reasons = reasons;
        }

        public IReadOnlyCollection<UnvalidatedOrderLine> Lines { get; }
        public IEnumerable<string> Reasons { get; }
    }

    /// <summary>
    /// Represents a validated order with all validation passed
    /// </summary>
    public record ValidatedOrder : IOrder
    {
        internal ValidatedOrder(IReadOnlyCollection<ValidatedOrderLine> lines,
            Guid userId,
            string deliveryAddress,
            string postalCode,
            string phone,
            string cardNumber,
            string expiry)
        {
            Lines = lines;
            UserId = userId;
            DeliveryAddress = deliveryAddress;
            PostalCode = postalCode;
            Phone = phone;
            CardNumber = cardNumber;
            Expiry = expiry;
        }

        public IReadOnlyCollection<ValidatedOrderLine> Lines { get; }
        public Guid UserId { get; }
        public string DeliveryAddress { get; }
        public string PostalCode { get; }
        public string Phone { get; }
        public string CardNumber { get; }
        public string Expiry { get; }
    }

    /// <summary>
    /// Represents an order with calculated total price
    /// </summary>
    public record PricedOrder : IOrder
    {
        internal PricedOrder(IReadOnlyCollection<ValidatedOrderLine> lines,
            Guid userId,
            string deliveryAddress,
            string postalCode,
            string phone,
            string cardNumber,
            string expiry,
            decimal totalPrice)
        {
            Lines = lines;
            UserId = userId;
            DeliveryAddress = deliveryAddress;
            PostalCode = postalCode;
            Phone = phone;
            CardNumber = cardNumber;
            Expiry = expiry;
            TotalPrice = totalPrice;
        }

        public IReadOnlyCollection<ValidatedOrderLine> Lines { get; }
        public Guid UserId { get; }
        public string DeliveryAddress { get; }
        public string PostalCode { get; }
        public string Phone { get; }
        public string CardNumber { get; }
        public string Expiry { get; }
        public decimal TotalPrice { get; }
    }

    /// <summary>
    /// Represents an order that has been persisted to the database
    /// </summary>
    public record PersistedOrder : IOrder
    {
        internal PersistedOrder(Guid orderId,
            IReadOnlyCollection<ValidatedOrderLine> lines,
            Guid userId,
            string deliveryAddress,
            string postalCode,
            string phone,
            decimal totalPrice,
            DateTime createdAt)
        {
            OrderId = orderId;
            Lines = lines;
            UserId = userId;
            DeliveryAddress = deliveryAddress;
            PostalCode = postalCode;
            Phone = phone;
            TotalPrice = totalPrice;
            CreatedAt = createdAt;
        }

        public Guid OrderId { get; }
        public IReadOnlyCollection<ValidatedOrderLine> Lines { get; }
        public Guid UserId { get; }
        public string DeliveryAddress { get; }
        public string PostalCode { get; }
        public string Phone { get; }
        public decimal TotalPrice { get; }
        public DateTime CreatedAt { get; }
    }

    /// <summary>
    /// Represents an order that has been published to the event bus
    /// </summary>
    public record PublishedOrder : IOrder
    {
        internal PublishedOrder(Guid orderId,
            IReadOnlyCollection<ValidatedOrderLine> lines,
            Guid userId,
            decimal totalPrice,
            DateTime publishedAt)
        {
            OrderId = orderId;
            Lines = lines;
            UserId = userId;
            TotalPrice = totalPrice;
            PublishedAt = publishedAt;
        }

        public Guid OrderId { get; }
        public IReadOnlyCollection<ValidatedOrderLine> Lines { get; }
        public Guid UserId { get; }
        public decimal TotalPrice { get; }
        public DateTime PublishedAt { get; }
    }
}

