namespace Invoicing.Events;

/// <summary>
/// Events that occur during invoice processing
/// </summary>
public static class InvoiceProcessedEvent
{
    public abstract record InvoiceProcessEvent;

    public record InvoiceCreatedSuccessfullyEvent : InvoiceProcessEvent
    {
        public Guid InvoiceId { get; init; }
        public string InvoiceNumber { get; init; } = string.Empty;
        public Guid ShipmentId { get; init; }
        public Guid OrderId { get; init; }
        public decimal TotalAmount { get; init; }
    }

    public record InvoiceCreationFailedEvent : InvoiceProcessEvent
    {
        public Guid ShipmentId { get; init; }
        public IEnumerable<string> Reasons { get; init; } = Enumerable.Empty<string>();
    }
}

