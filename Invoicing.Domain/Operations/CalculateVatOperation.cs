using static Invoicing.Models.Invoice;
using Invoicing.Models;

namespace Invoicing.Operations;

/// <summary>
/// Operation that calculates VAT/taxes on invoice lines (SYNC - pure transformation)
/// Business rules:
/// - Essential category: 11% VAT
/// - Electronics category: 21% VAT  
/// - Other categories: 21% VAT (default)
/// 
/// If discount exists, it's distributed proportionally across lines before VAT
/// </summary>
public sealed class CalculateVatOperation : InvoiceOperationWithState<Currency>
{
    protected override IInvoice OnCreated(CreatedInvoice invoice, Currency? displayCurrency)
    {
        // Calculate totals from lines (each line has its own VAT rate based on category)
        // LineNetAfterDiscount includes proportional discount
        var subTotal = new Money(invoice.Lines.Sum(l => l.LineNetAfterDiscount.Value));
        var totalVat = new Money(invoice.Lines.Sum(l => l.VatAmount.Value));
        var totalAmount = subTotal.Add(totalVat);

        return new CalculatedInvoice(
            Guid.NewGuid(),
            InvoiceNumber.Generate(),
            invoice.ShipmentId,
            invoice.OrderId,
            invoice.UserId,
            invoice.TrackingNumber,
            subTotal,
            totalVat,
            totalAmount,
            invoice.Lines,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(30), // Due in 30 days
            displayCurrency ?? Currency.Default());
    }
}

