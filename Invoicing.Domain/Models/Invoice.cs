using SharedKernel;

namespace Invoicing.Models;

/// <summary>
/// Invoice state enumeration for tracking current state
/// Note: Validation is done in Ordering domain. Invoice receives pre-validated data.
/// </summary>
public enum InvoiceState
{
    Created,       // Initial state from shipment event (data pre-validated by Ordering)
    Invalid,       // Validation failed (for consistency with DDD pattern)
    Calculated,    // Subtotal + VAT/taxes + total + invoiceNumber generated
    Persisted,     // Saved to DB (has InvoiceId)
    Published,     // InvoiceStateChanged sent to Service Bus
    Cancelled      // Invoice cancelled (due to order cancellation/return)
}

/// <summary>
/// Static class containing all invoice state records (DDD pattern)
/// </summary>
public static class Invoice
{
    /// <summary>
    /// Defines allowed state transitions for Invoice
    /// </summary>
    public static readonly StateTransitionMap<InvoiceState> Transitions = new StateTransitionMap<InvoiceState>()
        .Allow(InvoiceState.Created, InvoiceState.Calculated)
        .Allow(InvoiceState.Calculated, InvoiceState.Persisted)
        .Allow(InvoiceState.Persisted, InvoiceState.Published, InvoiceState.Cancelled)
        .Allow(InvoiceState.Published, InvoiceState.Cancelled); // Can cancel after publish (storno)

    public interface IInvoice : IStateMachine<InvoiceState>
    {
        InvoiceState IStateMachine<InvoiceState>.CurrentState => this switch
        {
            CreatedInvoice => InvoiceState.Created,
            InvalidInvoice => InvoiceState.Invalid,
            CalculatedInvoice => InvoiceState.Calculated,
            PersistedInvoice => InvoiceState.Persisted,
            PublishedInvoice => InvoiceState.Published,
            CancelledInvoice => InvoiceState.Cancelled,
            _ => throw new InvalidOperationException($"Unknown invoice state: {GetType().Name}")
        };

        bool IStateMachine<InvoiceState>.CanTransitionTo(InvoiceState targetState) =>
            Transitions.IsAllowed(((IStateMachine<InvoiceState>)this).CurrentState, targetState);
    }

    /// <summary>
    /// Represents a created invoice from shipment event
    /// Data comes pre-validated from Ordering/Shipment
    /// </summary>
    public record CreatedInvoice : IInvoice
    {
        public Guid ShipmentId { get; }
        public Guid OrderId { get; }
        public Guid UserId { get; }
        public TrackingNumber TrackingNumber { get; }
        public bool PremiumSubscription { get; }
        public Money TotalPrice { get; }
        public Money ShippingCost { get; }
        public Money TotalWithShipping { get; }
        public IReadOnlyCollection<InvoiceLine> Lines { get; }
        public DateTime ShipmentCreatedAt { get; }

        public CreatedInvoice(
            Guid shipmentId,
            Guid orderId,
            Guid userId,
            TrackingNumber trackingNumber,
            bool premiumSubscription,
            Money totalPrice,
            Money shippingCost,
            Money totalWithShipping,
            IReadOnlyCollection<InvoiceLine> lines,
            DateTime shipmentCreatedAt)
        {
            ShipmentId = shipmentId;
            OrderId = orderId;
            UserId = userId;
            TrackingNumber = trackingNumber;
            PremiumSubscription = premiumSubscription;
            TotalPrice = totalPrice;
            ShippingCost = shippingCost;
            TotalWithShipping = totalWithShipping;
            Lines = lines;
            ShipmentCreatedAt = shipmentCreatedAt;
        }

        /// <summary>
        /// Factory method to create from primitive values (from Shipment event)
        /// </summary>
        public static CreatedInvoice CreateFromEvent(
            Guid shipmentId,
            Guid orderId,
            Guid userId,
            string trackingNumber,
            bool premiumSubscription,
            decimal totalPrice,
            decimal shippingCost,
            decimal totalWithShipping,
            IReadOnlyCollection<InvoiceLine> lines,
            DateTime shipmentCreatedAt)
        {
            return new CreatedInvoice(
                shipmentId,
                orderId,
                userId,
                new TrackingNumber(trackingNumber),
                premiumSubscription,
                new Money(totalPrice),
                new Money(shippingCost),
                new Money(totalWithShipping),
                lines,
                shipmentCreatedAt);
        }
    }

    /// <summary>
    /// Represents a calculated invoice with subtotal, tax, and invoice number
    /// Values are stored in RON. EUR is derived for presentation.
    /// </summary>
    public record CalculatedInvoice : IInvoice
    {
        public Guid InvoiceId { get; }
        public InvoiceNumber InvoiceNumber { get; }
        public Guid ShipmentId { get; }
        public Guid OrderId { get; }
        public Guid UserId { get; }
        public TrackingNumber TrackingNumber { get; }
        public Money SubTotal { get; }
        public Money Tax { get; }
        public Money TotalAmount { get; }
        public IReadOnlyCollection<InvoiceLine> Lines { get; }
        public DateTime InvoiceDate { get; }
        public DateTime DueDate { get; }
        
        // Currency support - DB stores RON, EUR is derived
        public Currency DisplayCurrency { get; }
        public decimal TotalInRon { get; }
        public decimal TotalInEur { get; }

        public CalculatedInvoice(
            Guid invoiceId,
            InvoiceNumber invoiceNumber,
            Guid shipmentId,
            Guid orderId,
            Guid userId,
            TrackingNumber trackingNumber,
            Money subTotal,
            Money tax,
            Money totalAmount,
            IReadOnlyCollection<InvoiceLine> lines,
            DateTime invoiceDate,
            DateTime dueDate,
            Currency? displayCurrency = null)
        {
            InvoiceId = invoiceId;
            InvoiceNumber = invoiceNumber;
            ShipmentId = shipmentId;
            OrderId = orderId;
            UserId = userId;
            TrackingNumber = trackingNumber;
            SubTotal = subTotal;
            Tax = tax;
            TotalAmount = totalAmount;
            Lines = lines;
            InvoiceDate = invoiceDate;
            DueDate = dueDate;
            
            // Currency support - DB stores RON, EUR is derived
            DisplayCurrency = displayCurrency ?? Currency.Default();
            TotalInRon = totalAmount.Value;
            TotalInEur = CurrencyConverter.ConvertRonToEur(totalAmount.Value);
        }

        /// <summary>
        /// Create from CreatedInvoice with VAT calculations per line
        /// VAT rates: Essential = 11%, Electronics/Other = 21%
        /// </summary>
        public static CalculatedInvoice FromCreated(CreatedInvoice created, Currency? displayCurrency = null)
        {
            // Calculate totals from lines (each line has its own VAT rate based on category)
            // Use LineNetAfterDiscount which includes proportional discount
            var subTotal = new Money(created.Lines.Sum(l => (decimal)l.LineNetAfterDiscount.Value));
            var totalVat = new Money(created.Lines.Sum(l => (decimal)l.VatAmount.Value));
            var totalAmount = subTotal.Add(totalVat);

            return new CalculatedInvoice(
                Guid.NewGuid(),
                InvoiceNumber.Generate(),
                created.ShipmentId,
                created.OrderId,
                created.UserId,
                created.TrackingNumber,
                subTotal,
                totalVat,
                totalAmount,
                created.Lines,
                DateTime.UtcNow,
                DateTime.UtcNow.AddDays(30),
                displayCurrency ?? Currency.Default());
        }
    }

    /// <summary>
    /// Represents a persisted invoice (saved to database)
    /// </summary>
    public record PersistedInvoice : IInvoice
    {
        public Guid InvoiceId { get; }
        public InvoiceNumber InvoiceNumber { get; }
        public Guid ShipmentId { get; }
        public Guid OrderId { get; }
        public Guid UserId { get; }
        public TrackingNumber TrackingNumber { get; }
        public Money SubTotal { get; }
        public Money Tax { get; }
        public Money TotalAmount { get; }
        public IReadOnlyCollection<InvoiceLine> Lines { get; }
        public DateTime InvoiceDate { get; }
        public DateTime DueDate { get; }
        public DateTime PersistedAt { get; }

        public PersistedInvoice(CalculatedInvoice calculated, DateTime persistedAt)
        {
            InvoiceId = calculated.InvoiceId;
            InvoiceNumber = calculated.InvoiceNumber;
            ShipmentId = calculated.ShipmentId;
            OrderId = calculated.OrderId;
            UserId = calculated.UserId;
            TrackingNumber = calculated.TrackingNumber;
            SubTotal = calculated.SubTotal;
            Tax = calculated.Tax;
            TotalAmount = calculated.TotalAmount;
            Lines = calculated.Lines;
            InvoiceDate = calculated.InvoiceDate;
            DueDate = calculated.DueDate;
            PersistedAt = persistedAt;
        }
    }

    /// <summary>
    /// Represents a published invoice (InvoiceStateChanged sent to Service Bus)
    /// </summary>
    public record PublishedInvoice : IInvoice
    {
        public Guid InvoiceId { get; }
        public InvoiceNumber InvoiceNumber { get; }
        public Guid ShipmentId { get; }
        public Guid OrderId { get; }
        public Guid UserId { get; }
        public Money TotalAmount { get; }
        public DateTime PublishedAt { get; }

        public PublishedInvoice(PersistedInvoice persisted, DateTime publishedAt)
        {
            InvoiceId = persisted.InvoiceId;
            InvoiceNumber = persisted.InvoiceNumber;
            ShipmentId = persisted.ShipmentId;
            OrderId = persisted.OrderId;
            UserId = persisted.UserId;
            TotalAmount = persisted.TotalAmount;
            PublishedAt = publishedAt;
        }
    }

    /// <summary>
    /// Represents a cancelled invoice (storno)
    /// </summary>
    public record CancelledInvoice : IInvoice
    {
        public Guid InvoiceId { get; }
        public InvoiceNumber InvoiceNumber { get; }
        public Guid OrderId { get; }
        public string CancellationReason { get; }
        public DateTime CancelledAt { get; }

        public CancelledInvoice(
            Guid invoiceId,
            InvoiceNumber invoiceNumber,
            Guid orderId,
            string cancellationReason,
            DateTime cancelledAt)
        {
            InvoiceId = invoiceId;
            InvoiceNumber = invoiceNumber;
            OrderId = orderId;
            CancellationReason = cancellationReason;
            CancelledAt = cancelledAt;
        }

        /// <summary>
        /// Create from PersistedInvoice when order is cancelled/returned
        /// </summary>
        public static CancelledInvoice FromPersisted(PersistedInvoice persisted, string reason)
        {
            return new CancelledInvoice(
                persisted.InvoiceId,
                persisted.InvoiceNumber,
                persisted.OrderId,
                reason,
                DateTime.UtcNow);
        }
    }

    /// <summary>
    /// Represents an invalid invoice (validation failed)
    /// Note: Usually invoice data comes pre-validated from Ordering,
    /// but this state exists for consistency with the DDD pattern
    /// </summary>
    public record InvalidInvoice : IInvoice
    {
        public Guid OrderId { get; }
        public IEnumerable<string> Reasons { get; }

        public InvalidInvoice(Guid orderId, IEnumerable<string> reasons)
        {
            OrderId = orderId;
            Reasons = reasons;
        }

        public InvalidInvoice(Guid orderId, string reason)
            : this(orderId, new[] { reason })
        {
        }
    }
}

