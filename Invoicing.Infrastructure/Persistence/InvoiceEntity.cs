namespace Invoicing.Infrastructure.Persistence;

/// <summary>
/// Database entity for Invoice
/// </summary>
public class InvoiceEntity
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid ShipmentId { get; set; }
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal Tax { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<InvoiceLineEntity> Lines { get; set; } = new List<InvoiceLineEntity>();
}

