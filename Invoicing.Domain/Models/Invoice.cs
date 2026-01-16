namespace Invoicing.Models;

/// <summary>
/// Static class containing all invoice state records (DDD pattern)
/// </summary>
public static class Invoice
{
    public interface IInvoice { }

    /// <summary>
    /// Represents an unprocessed invoice from shipment event
    /// </summary>
    public record UnprocessedInvoice : IInvoice
    {
        public Guid ShipmentId { get; }
        public Guid OrderId { get; }
        public Guid UserId { get; }
        public string TrackingNumber { get; }
        public decimal TotalPrice { get; }
        public IReadOnlyCollection<InvoiceLine> Lines { get; }
        public DateTime ShipmentCreatedAt { get; }

        public UnprocessedInvoice(
            Guid shipmentId,
            Guid orderId,
            Guid userId,
            string trackingNumber,
            decimal totalPrice,
            IReadOnlyCollection<InvoiceLine> lines,
            DateTime shipmentCreatedAt)
        {
            ShipmentId = shipmentId;
            OrderId = orderId;
            UserId = userId;
            TrackingNumber = trackingNumber;
            TotalPrice = totalPrice;
            Lines = lines;
            ShipmentCreatedAt = shipmentCreatedAt;
        }
    }

    /// <summary>
    /// Represents an invoice that failed validation
    /// </summary>
    public record InvalidInvoice : IInvoice
    {
        public Guid ShipmentId { get; }
        public IEnumerable<string> Reasons { get; }

        public InvalidInvoice(Guid shipmentId, IEnumerable<string> reasons)
        {
            ShipmentId = shipmentId;
            Reasons = reasons;
        }
    }

    /// <summary>
    /// Represents a validated invoice
    /// </summary>
    public record ValidatedInvoice : IInvoice
    {
        public Guid ShipmentId { get; }
        public Guid OrderId { get; }
        public Guid UserId { get; }
        public string TrackingNumber { get; }
        public decimal TotalPrice { get; }
        public IReadOnlyCollection<InvoiceLine> Lines { get; }
        public DateTime ShipmentCreatedAt { get; }

        public ValidatedInvoice(
            Guid shipmentId,
            Guid orderId,
            Guid userId,
            string trackingNumber,
            decimal totalPrice,
            IReadOnlyCollection<InvoiceLine> lines,
            DateTime shipmentCreatedAt)
        {
            ShipmentId = shipmentId;
            OrderId = orderId;
            UserId = userId;
            TrackingNumber = trackingNumber;
            TotalPrice = totalPrice;
            Lines = lines;
            ShipmentCreatedAt = shipmentCreatedAt;
        }
    }

    /// <summary>
    /// Represents a generated invoice ready for delivery
    /// </summary>
    public record GeneratedInvoice : IInvoice
    {
        public Guid InvoiceId { get; }
        public string InvoiceNumber { get; }
        public Guid ShipmentId { get; }
        public Guid OrderId { get; }
        public Guid UserId { get; }
        public string TrackingNumber { get; }
        public decimal SubTotal { get; }
        public decimal Tax { get; }
        public decimal TotalAmount { get; }
        public IReadOnlyCollection<InvoiceLine> Lines { get; }
        public DateTime InvoiceDate { get; }
        public DateTime DueDate { get; }

        public GeneratedInvoice(
            Guid invoiceId,
            string invoiceNumber,
            Guid shipmentId,
            Guid orderId,
            Guid userId,
            string trackingNumber,
            decimal subTotal,
            decimal tax,
            decimal totalAmount,
            IReadOnlyCollection<InvoiceLine> lines,
            DateTime invoiceDate,
            DateTime dueDate)
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
        }
    }
}

