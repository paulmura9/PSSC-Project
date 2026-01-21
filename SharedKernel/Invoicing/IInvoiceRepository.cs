namespace SharedKernel.Invoicing;

/// <summary>
/// Interface for invoice repository operations
/// Port in SharedKernel, implemented in Infrastructure
/// </summary>
public interface IInvoiceRepository
{
    Task SaveInvoiceAsync(InvoiceSaveData invoice, CancellationToken cancellationToken = default);
    Task<InvoiceQueryResult?> GetByIdAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    Task<InvoiceQueryResult?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task UpdateStatusByOrderIdAsync(Guid orderId, string newStatus, CancellationToken cancellationToken = default);
}

/// <summary>
/// DTO for saving invoice data to repository
/// </summary>
public record InvoiceSaveData
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
    public string Status { get; init; } = "Pending";
    public DateTime InvoiceDate { get; init; }
    public DateTime DueDate { get; init; }
    public IReadOnlyCollection<InvoiceLineSaveData> Lines { get; init; } = Array.Empty<InvoiceLineSaveData>();
}

/// <summary>
/// DTO for saving invoice line data
/// </summary>
public record InvoiceLineSaveData
{
    public Guid InvoiceLineId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Category { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal LineTotal { get; init; }
    public decimal VatRate { get; init; }
    public decimal VatAmount { get; init; }
}

/// <summary>
/// DTO for invoice data returned from repository
/// </summary>
public record InvoiceQueryResult
{
    public Guid InvoiceId { get; init; }
    public string InvoiceNumber { get; init; } = string.Empty;
    public Guid ShipmentId { get; init; }
    public Guid OrderId { get; init; }
    public Guid UserId { get; init; }
    public decimal SubTotal { get; init; }
    public decimal Tax { get; init; }
    public decimal TotalAmount { get; init; }
    public string Status { get; init; } = "Pending";
    public DateTime InvoiceDate { get; init; }
    public DateTime DueDate { get; init; }
}

