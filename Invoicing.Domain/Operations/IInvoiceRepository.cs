namespace Invoicing.Operations;

/// <summary>
/// Interface for invoice repository operations
/// Port in Domain layer, implemented in Infrastructure
/// </summary>
public interface IInvoiceRepository
{
    /// <summary>
    /// Gets an invoice by its ID
    /// </summary>
    Task<InvoiceQueryResult?> GetByIdAsync(Guid invoiceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an invoice by order ID
    /// </summary>
    Task<InvoiceQueryResult?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the status of an invoice by order ID
    /// </summary>
    Task UpdateStatusByOrderIdAsync(Guid orderId, string newStatus, CancellationToken cancellationToken = default);
}

/// <summary>
/// DTO for invoice data returned from repository
/// Used to avoid dependency on Infrastructure entities in Domain
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

