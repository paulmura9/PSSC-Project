namespace Invoicing.Infrastructure.Persistence;

/// <summary>
/// Database entity for Invoice Line
/// </summary>
public class InvoiceLineEntity
{
    public Guid InvoiceLineId { get; set; }
    public Guid InvoiceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public InvoiceEntity Invoice { get; set; } = null!;
}

