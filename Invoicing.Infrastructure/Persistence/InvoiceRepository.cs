using Microsoft.EntityFrameworkCore;
namespace Invoicing.Infrastructure.Persistence;
public interface IInvoiceRepository
{
    Task SaveInvoiceAsync(InvoiceEntity invoice, IEnumerable<InvoiceLineEntity> lines, CancellationToken cancellationToken = default);
    Task<InvoiceEntity?> GetByIdAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    Task<InvoiceEntity?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
}
public class InvoiceRepository : IInvoiceRepository
{
    private readonly InvoicingDbContext _context;
    public InvoiceRepository(InvoicingDbContext context)
    {
        _context = context;
    }
    public async Task SaveInvoiceAsync(InvoiceEntity invoice, IEnumerable<InvoiceLineEntity> lines, CancellationToken cancellationToken = default)
    {
        invoice.Lines = lines.ToList();
        await _context.Invoices.AddAsync(invoice, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
    public async Task<InvoiceEntity?> GetByIdAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices.Include(i => i.Lines).FirstOrDefaultAsync(i => i.InvoiceId == invoiceId, cancellationToken);
    }
    public async Task<InvoiceEntity?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices.Include(i => i.Lines).FirstOrDefaultAsync(i => i.OrderId == orderId, cancellationToken);
    }
}
