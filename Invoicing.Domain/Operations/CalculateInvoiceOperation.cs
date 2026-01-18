using Invoicing.Models;
using Microsoft.Extensions.Logging;
using static Invoicing.Models.Invoice;

namespace Invoicing.Operations;

/// <summary>
/// Calculates an invoice - transforms CreatedInvoice to CalculatedInvoice
/// Generates invoice number, calculates VAT per line based on category:
/// - Essential products: 11% VAT
/// - Electronics/Other: 21% VAT
/// </summary>
public class CalculateInvoiceOperation
{
    private readonly ILogger<CalculateInvoiceOperation> _logger;

    public CalculateInvoiceOperation(ILogger<CalculateInvoiceOperation> logger)
    {
        _logger = logger;
    }

    public Task<IInvoice> ExecuteAsync(CreatedInvoice invoice, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("CalculateInvoiceOperation: Processing {LineCount} lines", invoice.Lines.Count);

        // Log VAT calculation per line
        foreach (var line in invoice.Lines)
        {
            _logger.LogInformation("  Line: {Name}, Category: {Category}, VAT Rate: {VatRate}%, Net: {Net}, Discount: {Disc}, AfterDisc: {After}, VAT: {Vat}, Total: {Total}",
                line.Name.Value,
                line.Category,
                line.VatRate.Percentage,
                line.LineNetInitial.Value,
                line.LineDiscount.Value,
                line.LineNetAfterDiscount.Value,
                line.VatAmount.Value,
                line.LineTotalWithVat.Value);
        }

        // Use factory method to calculate (VAT is calculated per line in InvoiceLine)
        var calculated = CalculatedInvoice.FromCreated(invoice);

        _logger.LogInformation("CalculatedInvoice: SubTotal={SubTotal}, TotalVAT={TotalVat}, TotalWithVAT={TotalAmount}",
            calculated.SubTotal.Value, calculated.Tax.Value, calculated.TotalAmount.Value);

        return Task.FromResult<IInvoice>(calculated);
    }
}

