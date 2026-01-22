using static Invoicing.Models.Invoice;
using Invoicing.Models;

namespace Invoicing.Operations;

/// <summary>
/// Operation that finalizes an invoice (SYNC - pure transformation)
/// Transforms VatCalculatedInvoice -> CalculatedInvoice
/// 
/// Business rules:
/// - Generates InvoiceId and InvoiceNumber
/// - Calculates TotalAmount (SubTotal + VAT)
/// - Sets DueDate based on customer type:
///   - Premium customers: 45 days
///   - Regular customers: 30 days
/// </summary>
public sealed class FinalizeInvoiceOperation : InvoiceOperation
{
    protected override IInvoice OnVatCalculated(VatCalculatedInvoice invoice)
    {
        // Calculate total amount (products with VAT + shipping)
        var totalAmount = invoice.SubTotal.Add(invoice.TotalVat).Add(invoice.ShippingCost);
        
        // Due date: Premium gets 45 days, Regular gets 30 days
        var dueDays = invoice.PremiumSubscription ? 45 : 30;
        var dueDate = DateTime.UtcNow.AddDays(dueDays);

        return new CalculatedInvoice(
            Guid.NewGuid(),
            InvoiceNumber.Generate(),
            invoice.ShipmentId,
            invoice.OrderId,
            invoice.UserId,
            invoice.TrackingNumber,
            invoice.SubTotal,
            invoice.TotalVat,
            totalAmount,
            invoice.Lines,
            DateTime.UtcNow,
            dueDate,
            Currency.Default());
    }
}

