namespace SharedKernel;

/// <summary>
/// Event published when an invoice is created successfully
/// </summary>
public record InvoiceCreatedEvent : IntegrationEvent
{
    public Guid InvoiceId { get; init; }
    public string InvoiceNumber { get; init; } = string.Empty;
    public Guid ShipmentId { get; init; }
    public Guid OrderId { get; init; }
    public Guid UserId { get; init; }
    public string TrackingNumber { get; init; } = string.Empty;
    public decimal SubTotal { get; init; }
    public decimal Tax { get; init; }
    public decimal TotalAmount { get; init; }
    public DateTime InvoiceDate { get; init; }
    public DateTime DueDate { get; init; }
    public List<InvoiceLineItem> Lines { get; init; } = new();
}

public record InvoiceLineItem
{
    public string Name { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal LineTotal { get; init; }
}

