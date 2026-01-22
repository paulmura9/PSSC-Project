﻿﻿using Microsoft.EntityFrameworkCore;
using Invoicing.Infrastructure.Persistence;
using SharedKernel.Invoicing;

namespace Invoicing.Infrastructure.Repository;

/// <summary>
/// Repository implementation for Invoice persistence operations
/// Maps EF entities to domain models 
/// </summary>
public class InvoiceRepository : IInvoiceRepository
{
    private readonly InvoicingDbContext _context;

    public InvoiceRepository(InvoicingDbContext context)
    {
        _context = context;
    }

    public async Task SaveInvoiceAsync(InvoiceSaveData invoice, CancellationToken cancellationToken = default)
    {
        // Check if invoice already exists (idempotency check for Service Bus retries)
        var existingInvoice = await _context.Invoices
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.InvoiceId == invoice.InvoiceId || i.ShipmentId == invoice.ShipmentId, cancellationToken);
        
        if (existingInvoice != null)
        {
            // Invoice already exists, skip (idempotent operation)
            return;
        }

        // Map domain DTO to EF entity 
        var entity = new InvoiceEntity
        {
            InvoiceId = invoice.InvoiceId,
            InvoiceNumber = invoice.InvoiceNumber,
            ShipmentId = invoice.ShipmentId,
            OrderId = invoice.OrderId,
            UserId = invoice.UserId,
            TrackingNumber = invoice.TrackingNumber,
            SubTotal = invoice.SubTotal,
            Tax = invoice.Tax,
            TotalAmount = invoice.TotalAmount,
            Status = invoice.Status,
            InvoiceDate = invoice.InvoiceDate,
            DueDate = invoice.DueDate,
            CreatedAt = DateTime.UtcNow,
            Lines = invoice.Lines.Select(l => new InvoiceLineEntity
            {
                InvoiceLineId = l.InvoiceLineId,
                InvoiceId = invoice.InvoiceId,
                Name = l.Name,
                Description = l.Description,
                Category = l.Category,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                LineTotal = l.LineTotal,
                VatRate = l.VatRate,
                VatAmount = l.VatAmount
            }).ToList()
        };

        //salvez in db
        await _context.Invoices.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<InvoiceQueryResult?> GetByIdAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Invoices
            .AsNoTracking()
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId, cancellationToken);

        if (entity == null)
            return null;

        // Map entity to domain model 
        return MapToQueryResult(entity);
    }

    public async Task<InvoiceQueryResult?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Invoices
            .AsNoTracking()
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.OrderId == orderId, cancellationToken);

        if (entity == null)
            return null;

        // Map entity to domain model 
        return MapToQueryResult(entity);
    }

    public async Task UpdateStatusByOrderIdAsync(Guid orderId, string newStatus, CancellationToken cancellationToken = default)
    {
        var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.OrderId == orderId, cancellationToken);
        if (invoice != null)
        {
            invoice.Status = newStatus;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Maps EF entity to domain query result (lab pattern)
    /// </summary>
    private static InvoiceQueryResult MapToQueryResult(InvoiceEntity entity)
    {
        return new InvoiceQueryResult
        {
            InvoiceId = entity.InvoiceId,
            InvoiceNumber = entity.InvoiceNumber,
            ShipmentId = entity.ShipmentId,
            OrderId = entity.OrderId,
            UserId = entity.UserId,
            SubTotal = entity.SubTotal,
            Tax = entity.Tax,
            TotalAmount = entity.TotalAmount,
            Status = entity.Status,
            InvoiceDate = entity.InvoiceDate,
            DueDate = entity.DueDate
        };
    }
}
