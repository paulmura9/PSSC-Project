﻿using Microsoft.EntityFrameworkCore;
using Invoicing.Infrastructure.Persistence;

namespace Invoicing.Infrastructure.Repository;

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

    public async Task UpdateStatusByOrderIdAsync(Guid orderId, string newStatus, CancellationToken cancellationToken = default)
    {
        var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.OrderId == orderId, cancellationToken);
        if (invoice != null)
        {
            invoice.Status = newStatus;
            invoice.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
