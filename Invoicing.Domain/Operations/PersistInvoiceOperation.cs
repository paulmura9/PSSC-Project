using SharedKernel.Invoicing;
using static Invoicing.Models.Invoice;

namespace Invoicing.Operations;

/// <summary>
/// Persists a CalculatedInvoice to the database and returns PersistedInvoice
/// CalculatedInvoice -> PersistedInvoice
/// ASYNC - requires I/O (Database)
/// </summary>
public class PersistInvoiceOperation
{
    private readonly IInvoiceRepository _repository;

    public PersistInvoiceOperation(IInvoiceRepository repository)
    {
        _repository = repository;
    }

    public async Task<IInvoice> ExecuteAsync(CalculatedInvoice invoice, string paymentStatus, CancellationToken cancellationToken = default)
    {
        var saveData = MapToSaveData(invoice, paymentStatus);
        await _repository.SaveInvoiceAsync(saveData, cancellationToken);

        return new PersistedInvoice(invoice, DateTime.UtcNow);
    }

    private static InvoiceSaveData MapToSaveData(CalculatedInvoice calculated, string paymentStatus)
    {
        return new InvoiceSaveData
        {
            InvoiceId = calculated.InvoiceId,
            InvoiceNumber = calculated.InvoiceNumber.Value,
            ShipmentId = calculated.ShipmentId,
            OrderId = calculated.OrderId,
            UserId = calculated.UserId,
            TrackingNumber = calculated.TrackingNumber.Value,
            SubTotal = calculated.SubTotal.Value,
            Tax = calculated.Tax.Value,
            TotalAmount = calculated.TotalAmount.Value,
            Status = paymentStatus,
            InvoiceDate = calculated.InvoiceDate,
            DueDate = calculated.DueDate,
            Lines = calculated.Lines.Select(l => new InvoiceLineSaveData
            {
                InvoiceLineId = Guid.NewGuid(),
                Name = l.Name.Value,
                Description = l.Description.Value,
                Category = l.Category.ToString(),
                Quantity = l.Quantity.Value,
                UnitPrice = l.UnitPrice.Value,
                LineTotal = l.LineTotalWithVat.Value,
                VatRate = l.VatRate.Percentage / 100m,
                VatAmount = l.VatAmount.Value
            }).ToList().AsReadOnly()
        };
    }
}
