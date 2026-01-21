using SharedKernel;
using SharedKernel.StateMachine;

namespace Ordering.Domain.Models;

/// <summary>
/// Order state enumeration for tracking current state
/// </summary>
public enum OrderState
{
    Unvalidated,
    Invalid,
    Validated,
    Priced,
    Persistable,
    Persisted,
    Published
}

/// <summary>
/// Static class containing all order state records (similar to Exam.cs in lab)
/// </summary>
public static class Order
{
    /// <summary>
    /// Defines allowed state transitions for Order
    /// Simple linear flow: Unvalidated → Validated → Priced → Persistable → Persisted → Published
    /// </summary>
    public static readonly StateTransitionMap<OrderState> Transitions = new StateTransitionMap<OrderState>()
        .Allow(OrderState.Unvalidated, OrderState.Validated, OrderState.Invalid)
        .Allow(OrderState.Validated, OrderState.Priced, OrderState.Invalid)
        .Allow(OrderState.Priced, OrderState.Persistable)
        .Allow(OrderState.Persistable, OrderState.Persisted)
        .Allow(OrderState.Persisted, OrderState.Published);

    /// <summary>
    /// Marker interface for all order states
    /// </summary>
    public interface IOrder : IStateMachine<OrderState>
    {
        OrderState IStateMachine<OrderState>.CurrentState => this switch
        {
            UnvalidatedOrder => OrderState.Unvalidated,
            InvalidOrder => OrderState.Invalid,
            ValidatedOrder => OrderState.Validated,
            PricedOrder => OrderState.Priced,
            PersistableOrder => OrderState.Persistable,
            PersistedOrder => OrderState.Persisted,
            PublishedOrder => OrderState.Published,
            _ => throw new InvalidOperationException($"Unknown order state: {GetType().Name}")
        };

        bool IStateMachine<OrderState>.CanTransitionTo(OrderState targetState) =>
            Transitions.IsAllowed(((IStateMachine<OrderState>)this).CurrentState, targetState);
    }

    /// <summary>
    /// Represents an unvalidated order from user input
    /// </summary>
    public record UnvalidatedOrder : IOrder
    {
        public UnvalidatedOrder(IReadOnlyCollection<UnvalidatedOrderLine> lines,
            Guid userId,
            string? street,
            string? city,
            string? postalCode,
            string phone,
            string? email,
            string? deliveryNotes,
            bool premiumSubscription,
            string pickupMethod,
            string? pickupPointId,
            string paymentMethod)
        {
            Lines = lines;
            UserId = userId;
            Street = street;
            City = city;
            PostalCode = postalCode;
            Phone = phone;
            Email = email;
            DeliveryNotes = deliveryNotes;
            PremiumSubscription = premiumSubscription;
            PickupMethodInput = pickupMethod;
            PickupPointIdInput = pickupPointId;
            PaymentMethodInput = paymentMethod;
        }

        public IReadOnlyCollection<UnvalidatedOrderLine> Lines { get; }
        public Guid UserId { get; }
        public string? Street { get; }
        public string? City { get; }
        public string? PostalCode { get; }
        public string Phone { get; }
        public string? Email { get; }
        public string? DeliveryNotes { get; }
        public bool PremiumSubscription { get; }
        public string PickupMethodInput { get; }
        public string? PickupPointIdInput { get; }
        public string PaymentMethodInput { get; }
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
    /// Uses Value Objects for validated data
    /// </summary>
    public record ValidatedOrder : IOrder
    {
        internal ValidatedOrder(IReadOnlyCollection<ValidatedOrderLine> lines,
            CustomerId userId,
            Street? street,
            City? city,
            PostalCode? postalCode,
            PhoneNumber phone,
            EmailAddress? email,
            DeliveryNotes? deliveryNotes,
            bool premiumSubscription,
            PickupMethod pickupMethod,
            PickupPointId? pickupPointId,
            PaymentMethod paymentMethod)
        {
            Lines = lines;
            UserId = userId;
            Street = street;
            City = city;
            PostalCode = postalCode;
            Phone = phone;
            Email = email;
            DeliveryNotes = deliveryNotes;
            PremiumSubscription = premiumSubscription;
            PickupMethod = pickupMethod;
            PickupPointId = pickupPointId;
            PaymentMethod = paymentMethod;
        }

        public IReadOnlyCollection<ValidatedOrderLine> Lines { get; }
        public CustomerId UserId { get; }
        public Street? Street { get; }
        public City? City { get; }
        public PostalCode? PostalCode { get; }
        public PhoneNumber Phone { get; }
        public EmailAddress? Email { get; }
        public DeliveryNotes? DeliveryNotes { get; }
        public bool PremiumSubscription { get; }
        public PickupMethod PickupMethod { get; }
        public PickupPointId? PickupPointId { get; }
        public PaymentMethod PaymentMethod { get; }
    }

    /// <summary>
    /// Represents an order with calculated total price (with optional voucher discount)
    /// Uses Value Objects for validated data
    /// </summary>
    public record PricedOrder : IOrder
    {
        internal PricedOrder(IReadOnlyCollection<ValidatedOrderLine> lines,
            CustomerId userId,
            Street? street,
            City? city,
            PostalCode? postalCode,
            PhoneNumber phone,
            EmailAddress? email,
            DeliveryNotes? deliveryNotes,
            decimal subtotal,
            decimal discountAmount,
            decimal total,
            VoucherCode? voucherCode,
            bool premiumSubscription,
            PickupMethod pickupMethod,
            PickupPointId? pickupPointId,
            PaymentMethod paymentMethod)
        {
            Lines = lines;
            UserId = userId;
            Street = street;
            City = city;
            PostalCode = postalCode;
            Phone = phone;
            Email = email;
            DeliveryNotes = deliveryNotes;
            Subtotal = subtotal;
            DiscountAmount = discountAmount;
            Total = total;
            VoucherCode = voucherCode;
            PremiumSubscription = premiumSubscription;
            PickupMethod = pickupMethod;
            PickupPointId = pickupPointId;
            PaymentMethod = paymentMethod;
        }

        public IReadOnlyCollection<ValidatedOrderLine> Lines { get; }
        public CustomerId UserId { get; }
        public Street? Street { get; }
        public City? City { get; }
        public PostalCode? PostalCode { get; }
        public PhoneNumber Phone { get; }
        public EmailAddress? Email { get; }
        public DeliveryNotes? DeliveryNotes { get; }
        public decimal Subtotal { get; }
        public decimal DiscountAmount { get; }
        public decimal Total { get; }
        public VoucherCode? VoucherCode { get; }
        public bool PremiumSubscription { get; }
        public PickupMethod PickupMethod { get; }
        public PickupPointId? PickupPointId { get; }
        public PaymentMethod PaymentMethod { get; }
        
    }

    /// <summary>
    /// Represents an order that has been persisted to the database
    /// Uses Value Objects for domain data
    /// </summary>
    public record PersistedOrder : IOrder
    {
        internal PersistedOrder(Guid orderId,
            IReadOnlyCollection<ValidatedOrderLine> lines,
            CustomerId userId,
            Street? street,
            City? city,
            PostalCode? postalCode,
            PhoneNumber phone,
            EmailAddress? email,
            decimal subtotal,
            decimal discountAmount,
            decimal total,
            VoucherCode? voucherCode,
            bool premiumSubscription,
            PickupMethod pickupMethod,
            PickupPointId? pickupPointId,
            PaymentMethod paymentMethod,
            DateTime createdAt)
        {
            OrderId = orderId;
            Lines = lines;
            UserId = userId;
            Street = street;
            City = city;
            PostalCode = postalCode;
            Phone = phone;
            Email = email;
            Subtotal = subtotal;
            DiscountAmount = discountAmount;
            Total = total;
            VoucherCode = voucherCode;
            PremiumSubscription = premiumSubscription;
            PickupMethod = pickupMethod;
            PickupPointId = pickupPointId;
            PaymentMethod = paymentMethod;
            CreatedAt = createdAt;
        }

        public Guid OrderId { get; }
        public IReadOnlyCollection<ValidatedOrderLine> Lines { get; }
        public CustomerId UserId { get; }
        public Street? Street { get; }
        public City? City { get; }
        public PostalCode? PostalCode { get; }
        public PhoneNumber Phone { get; }
        public EmailAddress? Email { get; }
        public decimal Subtotal { get; }
        public decimal DiscountAmount { get; }
        public decimal Total { get; }
        public VoucherCode? VoucherCode { get; }
        public bool PremiumSubscription { get; }
        public PickupMethod PickupMethod { get; }
        public PickupPointId? PickupPointId { get; }
        public PaymentMethod PaymentMethod { get; }
        public DateTime CreatedAt { get; }
        
    }

    /// <summary>
    /// Represents an order ready to be persisted (mapped to DB model)
    /// </summary>
    public record PersistableOrder : IOrder
    {
        internal PersistableOrder(
            IReadOnlyCollection<PersistableOrderLine> lines,
            Guid userId,
            string? street,
            string? city,
            string? postalCode,
            string phone,
            string? email,
            string? deliveryNotes,
            decimal subtotal,
            decimal discountAmount,
            decimal total,
            string? voucherCode,
            bool premiumSubscription,
            string pickupMethod,
            string? pickupPointId,
            string paymentMethod)
        {
            Lines = lines;
            UserId = userId;
            Street = street;
            City = city;
            PostalCode = postalCode;
            Phone = phone;
            Email = email;
            DeliveryNotes = deliveryNotes;
            Subtotal = subtotal;
            DiscountAmount = discountAmount;
            Total = total;
            VoucherCode = voucherCode;
            PremiumSubscription = premiumSubscription;
            PickupMethod = pickupMethod;
            PickupPointId = pickupPointId;
            PaymentMethod = paymentMethod;
        }

        public IReadOnlyCollection<PersistableOrderLine> Lines { get; }
        public Guid UserId { get; }
        public string? Street { get; }
        public string? City { get; }
        public string? PostalCode { get; }
        public string Phone { get; }
        public string? Email { get; }
        public string? DeliveryNotes { get; }
        public decimal Subtotal { get; }
        public decimal DiscountAmount { get; }
        public decimal Total { get; }
        public string? VoucherCode { get; }
        public bool PremiumSubscription { get; }
        public string PickupMethod { get; }
        public string? PickupPointId { get; }
        public string PaymentMethod { get; }

    }

    /// <summary>
    /// Represents a line in a persistable order
    /// </summary>
    public record PersistableOrderLine(
        string Name,
        string Description,
        string Category,
        int Quantity,
        decimal UnitPrice,
        decimal LineTotal);

    /// <summary>
    /// Represents an order that has been published to the event bus
    /// Uses Value Objects for domain data
    /// </summary>
    public record PublishedOrder : IOrder
    {
        internal PublishedOrder(
            Guid orderId,
            IReadOnlyCollection<ValidatedOrderLine> lines,
            CustomerId userId,
            Street? street,
            City? city,
            PostalCode? postalCode,
            PhoneNumber phone,
            EmailAddress? email,
            decimal subtotal,
            decimal discountAmount,
            decimal total,
            VoucherCode? voucherCode,
            bool premiumSubscription,
            PickupMethod pickupMethod,
            PickupPointId? pickupPointId,
            PaymentMethod paymentMethod,
            DateTime publishedAt)
        {
            OrderId = orderId;
            Lines = lines;
            UserId = userId;
            Street = street;
            City = city;
            PostalCode = postalCode;
            Phone = phone;
            Email = email;
            Subtotal = subtotal;
            DiscountAmount = discountAmount;
            Total = total;
            VoucherCode = voucherCode;
            PremiumSubscription = premiumSubscription;
            PickupMethod = pickupMethod;
            PickupPointId = pickupPointId;
            PaymentMethod = paymentMethod;
            PublishedAt = publishedAt;
        }

        public Guid OrderId { get; }
        public IReadOnlyCollection<ValidatedOrderLine> Lines { get; }
        public CustomerId UserId { get; }
        public Street? Street { get; }
        public City? City { get; }
        public PostalCode? PostalCode { get; }
        public PhoneNumber Phone { get; }
        public EmailAddress? Email { get; }
        public decimal Subtotal { get; }
        public decimal DiscountAmount { get; }
        public decimal Total { get; }
        public VoucherCode? VoucherCode { get; }
        public bool PremiumSubscription { get; }
        public PickupMethod PickupMethod { get; }
        public PickupPointId? PickupPointId { get; }
        public PaymentMethod PaymentMethod { get; }
        public DateTime PublishedAt { get; }
        
        // For backwards compatibility
        public decimal TotalPrice => Total;
    }
}


