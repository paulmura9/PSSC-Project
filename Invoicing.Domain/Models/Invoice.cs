using SharedKernel;
using SharedKernel.StateMachine;

namespace Invoicing.Models;

/// <summary>
/// Invoice state enumeration for tracking current state
/// Note: Validation is done in Ordering domain. Invoice receives pre-validated data.
/// </summary>
public enum InvoiceState
{
    Created,        // Initial state from shipment event (data pre-validated by Ordering)
    VatCalculated,  // VAT calculated per line
    Calculated,     // Subtotal + VAT/taxes + total + invoiceNumber + dueDate generated
    Persisted,      // Saved to DB (has InvoiceId)
    Published       // InvoiceStateChanged sent to Service Bus
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
        .Allow(InvoiceState.Created, InvoiceState.VatCalculated)
        .Allow(InvoiceState.VatCalculated, InvoiceState.Calculated)
        .Allow(InvoiceState.Calculated, InvoiceState.Persisted)
        .Allow(InvoiceState.Persisted, InvoiceState.Published);

    public interface IInvoice : IStateMachine<InvoiceState>
    {
        InvoiceState IStateMachine<InvoiceState>.CurrentState => this switch
        {
            CreatedInvoice => InvoiceState.Created,
            VatCalculatedInvoice => InvoiceState.VatCalculated,
            CalculatedInvoice => InvoiceState.Calculated,
            PersistedInvoice => InvoiceState.Persisted,
            PublishedInvoice => InvoiceState.Published,
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
    /// Represents an invoice with VAT calculated per line
    /// Intermediate state before final totals calculation
    /// </summary>
    public record VatCalculatedInvoice : IInvoice
    {
        public Guid ShipmentId { get; }
        public Guid OrderId { get; }
        public Guid UserId { get; }
        public TrackingNumber TrackingNumber { get; }
        public bool PremiumSubscription { get; }
        public Money SubTotal { get; }
        public Money TotalVat { get; }
        public Money ShippingCost { get; }
        public IReadOnlyCollection<InvoiceLine> Lines { get; }
        public DateTime ShipmentCreatedAt { get; }
        public DateTime VatCalculatedAt { get; }

        public VatCalculatedInvoice(
            Guid shipmentId,
            Guid orderId,
            Guid userId,
            TrackingNumber trackingNumber,
            bool premiumSubscription,
            Money subTotal,
            Money totalVat,
            Money shippingCost,
            IReadOnlyCollection<InvoiceLine> lines,
            DateTime shipmentCreatedAt,
            DateTime vatCalculatedAt)
        {
            ShipmentId = shipmentId;
            OrderId = orderId;
            UserId = userId;
            TrackingNumber = trackingNumber;
            PremiumSubscription = premiumSubscription;
            SubTotal = subTotal;
            TotalVat = totalVat;
            ShippingCost = shippingCost;
            Lines = lines;
            ShipmentCreatedAt = shipmentCreatedAt;
            VatCalculatedAt = vatCalculatedAt;
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
        public Currency DisplayCurrency { get; }
        public decimal TotalInRon { get; }
        public decimal TotalInEur { get; }

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
            DisplayCurrency = calculated.DisplayCurrency;
            TotalInRon = calculated.TotalInRon;
            TotalInEur = calculated.TotalInEur;
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
        public TrackingNumber TrackingNumber { get; }
        public Money SubTotal { get; }
        public Money Tax { get; }
        public Money TotalAmount { get; }
        public IReadOnlyCollection<InvoiceLine> Lines { get; }
        public DateTime InvoiceDate { get; }
        public DateTime DueDate { get; }
        public DateTime PublishedAt { get; }
        public Currency DisplayCurrency { get; }
        public decimal TotalInRon { get; }
        public decimal TotalInEur { get; }

        public PublishedInvoice(PersistedInvoice persisted, DateTime publishedAt)
        {
            InvoiceId = persisted.InvoiceId;
            InvoiceNumber = persisted.InvoiceNumber;
            ShipmentId = persisted.ShipmentId;
            OrderId = persisted.OrderId;
            UserId = persisted.UserId;
            TrackingNumber = persisted.TrackingNumber;
            SubTotal = persisted.SubTotal;
            Tax = persisted.Tax;
            TotalAmount = persisted.TotalAmount;
            Lines = persisted.Lines;
            InvoiceDate = persisted.InvoiceDate;
            DueDate = persisted.DueDate;
            PublishedAt = publishedAt;
            DisplayCurrency = persisted.DisplayCurrency;
            TotalInRon = persisted.TotalInRon;
            TotalInEur = persisted.TotalInEur;
        }
    }



    /// <summary>
    /// Extension method to convert invoice state to event (Lab-style pattern)
    /// </summary>
    public static Events.IInvoiceWorkflowResult ToEvent(this IInvoice invoice) => invoice switch
    {
        PersistedInvoice persisted => new Events.InvoiceCreatedSuccessEvent
        {
            InvoiceId = persisted.InvoiceId,
            InvoiceNumber = persisted.InvoiceNumber.Value,
            ShipmentId = persisted.ShipmentId,
            OrderId = persisted.OrderId,
            UserId = persisted.UserId,
            SubTotal = persisted.SubTotal.Value,
            Tax = persisted.Tax.Value,
            TotalAmount = persisted.TotalAmount.Value,
            CreatedAt = DateTime.UtcNow,
            Currency = persisted.DisplayCurrency.Value,
            TotalInRon = persisted.TotalInRon,
            TotalInEur = persisted.TotalInEur
        },
        PublishedInvoice published => new Events.InvoiceCreatedSuccessEvent
        {
            InvoiceId = published.InvoiceId,
            InvoiceNumber = published.InvoiceNumber.Value,
            ShipmentId = published.ShipmentId,
            OrderId = published.OrderId,
            UserId = published.UserId,
            SubTotal = published.SubTotal.Value,
            Tax = published.Tax.Value,
            TotalAmount = published.TotalAmount.Value,
            CreatedAt = published.PublishedAt,
            Currency = published.DisplayCurrency.Value,
            TotalInRon = published.TotalInRon,
            TotalInEur = published.TotalInEur
        },
        _ => new Events.InvoiceCreatedFailedEvent
        { 
            Reasons = new[] { $"Unexpected invoice state: {invoice.GetType().Name}" } 
        }
    };
}

