namespace SharedKernel;

/// <summary>
/// Event emitted by Invoicing context when invoice is created
/// Published to "invoices" topic
/// </summary>
public sealed record InvoiceStateChangedEvent : IntegrationEvent
{
    /// <summary>
    /// Current state: Created, Paid, Overdue
    /// </summary>
    public string InvoiceState { get; init; } = string.Empty;
    
    public Guid InvoiceId { get; init; }
    public string InvoiceNumber { get; init; } = string.Empty;
    public Guid OrderId { get; init; }
    public Guid ShipmentId { get; init; }
    public Guid UserId { get; init; }
    public string TrackingNumber { get; init; } = string.Empty;
    
    /// <summary>
    /// Payment status: "Authorized" for CardOnline, "Pending" otherwise
    /// </summary>
    public string PaymentStatus { get; init; } = "Pending";
    
    public decimal SubTotal { get; init; }
    public decimal Tax { get; init; }
    public decimal TotalAmount { get; init; }
    
    public DateTime InvoiceDate { get; init; }
    public DateTime DueDate { get; init; }
    
    /// <summary>
    /// Invoice line items
    /// </summary>
    public List<LineItemDto> Lines { get; init; } = new();
    
    /// <summary>
    /// Currency (RON)
    /// </summary>
    public string Currency { get; init; } = "RON";
    
    /// <summary>
    /// Total in RON
    /// </summary>
    public decimal TotalInRon { get; init; }
    
    /// <summary>
    /// Total in EUR (converted)
    /// </summary>
    public decimal TotalInEur { get; init; }

    public InvoiceStateChangedEvent() : base(Guid.NewGuid(), DateTime.UtcNow) { }
    
    public InvoiceStateChangedEvent(Guid eventId, DateTime occurredAt) : base(eventId, occurredAt) { }
}
