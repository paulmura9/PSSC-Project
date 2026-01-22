using static Invoicing.Models.Invoice;
using Invoicing.Models;

namespace Invoicing.Operations;

/// <summary>
/// Operation that calculates VAT/taxes on invoice lines (SYNC - pure transformation)
/// Transforms CreatedInvoice -> VatCalculatedInvoice
/// 
/// Business rules:
/// - Essential category: 11% VAT
/// - Electronics category: 21% VAT  
/// - Other categories: 21% VAT (default)
/// 
/// If discount exists, it's distributed proportionally across lines before VAT
/// </summary>
public sealed class CalculateVatOperation : InvoiceOperation
{
    protected override IInvoice OnCreated(CreatedInvoice invoice)
    {
        // calculate VAT si subtotal per line
        // LineNetAfterDiscount includes proportional discount
        var subTotal = new Money(invoice.Lines.Sum(l => l.LineNetAfterDiscount.Value));
        var totalVat = new Money(invoice.Lines.Sum(l => l.VatAmount.Value));

        return new VatCalculatedInvoice(
            invoice.ShipmentId,
            invoice.OrderId,
            invoice.UserId,
            invoice.TrackingNumber,
            invoice.PremiumSubscription,
            subTotal,
            totalVat,
            invoice.ShippingCost,
            invoice.Lines,
            invoice.ShipmentCreatedAt,
            DateTime.UtcNow);
    }
}
