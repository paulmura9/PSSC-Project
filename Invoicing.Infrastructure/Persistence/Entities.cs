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
/// <summary>
/// Database entity for Invoice Line
/// </summary>
public class InvoiceLineEntity
{
    public Guid InvoiceLineId { get; set; }
    public Guid InvoiceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public InvoiceEntity Invoice { get; set; } = null!;
}
