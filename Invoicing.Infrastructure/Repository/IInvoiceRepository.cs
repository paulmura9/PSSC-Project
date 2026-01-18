using Invoicing.Infrastructure.Persistence;

namespace Invoicing.Infrastructure.Repository;

public interface IInvoiceRepository
{
    Task SaveInvoiceAsync(InvoiceEntity invoice, IEnumerable<InvoiceLineEntity> lines, CancellationToken cancellationToken = default);
    Task<InvoiceEntity?> GetByIdAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    Task<InvoiceEntity?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task UpdateStatusByOrderIdAsync(Guid orderId, string newStatus, CancellationToken cancellationToken = default);
}

