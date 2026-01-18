namespace Invoicing.Events;

/// <summary>
/// Events that occur when an invoice is generated
/// </summary>
public interface IInvoiceGeneratedEvent { }

/// <summary>
/// Event indicating invoice was generated successfully
/// Values are calculated in RON. EUR is derived for presentation.
/// </summary>
public record InvoiceGeneratedEvent : IInvoiceGeneratedEvent
{
    public Guid InvoiceId { get; init; }
    public string InvoiceNumber { get; init; } = string.Empty;
    public Guid ShipmentId { get; init; }
    public Guid OrderId { get; init; }
    public Guid UserId { get; init; }
    public decimal SubTotal { get; init; }
    public decimal Tax { get; init; }
    public decimal TotalAmount { get; init; }
    public DateTime GeneratedAt { get; init; }
    
    // Currency support - DB stores RON, EUR is derived
    public string Currency { get; init; } = "RON";
    public decimal TotalInRon { get; init; }
    public decimal? TotalInEur { get; init; }
}

/// <summary>
/// Event indicating invoice generation failed
/// </summary>
public record InvoiceGenerationFailedEvent : IInvoiceGeneratedEvent
{
    public Guid ShipmentId { get; init; }
    public Guid OrderId { get; init; }
    public IEnumerable<string> Reasons { get; init; } = Enumerable.Empty<string>();
}

