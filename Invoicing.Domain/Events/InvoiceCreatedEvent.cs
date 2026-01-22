using SharedKernel;

namespace Invoicing.Events;

/// <summary>
/// Event published when an invoice is created successfully
/// Values are stored in RON. EUR is derived for presentation.
/// </summary>
public sealed record InvoiceCreatedEvent : IntegrationEvent
{
    public Guid InvoiceId { get; init; }
    public string InvoiceNumber { get; init; } = string.Empty;
    public Guid ShipmentId { get; init; }
    public Guid OrderId { get; init; }
    public Guid UserId { get; init; }
    public string TrackingNumber { get; init; } = string.Empty;
    
    /// <summary>
    /// Subtotal before VAT
    /// </summary>
    public decimal SubTotal { get; init; }
    
    /// <summary>
    /// Total VAT amount
    /// </summary>
    public decimal Tax { get; init; }
    
    /// <summary>
    /// Total with VAT (SubTotal + Tax)
    /// </summary>
    public decimal TotalAmount { get; init; }
    
    public DateTime InvoiceDate { get; init; }
    public DateTime DueDate { get; init; }
    public List<LineItemDto> Lines { get; init; } = new();
    
    /// <summary>
    /// Currency code (RON, EUR)
    /// </summary>
    public string Currency { get; init; } = "RON";
    
    /// <summary>
    /// Total in RON (always populated)
    /// </summary>
    public decimal TotalInRon { get; init; }
    
    /// <summary>
    /// Total in EUR (derived, 1 RON = 0.20 EUR)
    /// </summary>
    public decimal? TotalInEur { get; init; }
    
    /// <summary>
    /// Payment status (Pending, Authorized)
    /// </summary>
    public string PaymentStatus { get; init; } = "Pending";

    public InvoiceCreatedEvent() : base() { }
    
    public InvoiceCreatedEvent(Guid eventId, DateTime occurredAt) : base(eventId, occurredAt) { }
}

