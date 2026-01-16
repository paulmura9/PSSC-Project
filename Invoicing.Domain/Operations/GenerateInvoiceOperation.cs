using static Invoicing.Models.Invoice;

namespace Invoicing.Operations;

/// <summary>
/// Generates an invoice - transforms ValidatedInvoice to GeneratedInvoice
/// </summary>
public class GenerateInvoiceOperation
{
    private const decimal TaxRate = 0.21m; // 21% tax rate

    public Task<IInvoice> ExecuteAsync(ValidatedInvoice invoice, CancellationToken cancellationToken = default)
    {
        var invoiceId = Guid.NewGuid();
        var invoiceNumber = GenerateInvoiceNumber(invoiceId);
        var invoiceDate = DateTime.UtcNow;
        var dueDate = invoiceDate.AddDays(30); // 30 days payment term

        var subTotal = invoice.TotalPrice;
        var tax = Math.Round(subTotal * TaxRate, 2);
        var totalAmount = subTotal + tax;

        return Task.FromResult<IInvoice>(new GeneratedInvoice(
            invoiceId,
            invoiceNumber,
            invoice.ShipmentId,
            invoice.OrderId,
            invoice.UserId,
            invoice.TrackingNumber,
            subTotal,
            tax,
            totalAmount,
            invoice.Lines,
            invoiceDate,
            dueDate));
    }

    private static string GenerateInvoiceNumber(Guid invoiceId)
    {
        return $"INV-{DateTime.UtcNow:yyyyMMdd}-{invoiceId.ToString()[..8].ToUpper()}";
    }
}

