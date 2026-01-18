using Invoicing.Events;
using Invoicing.Models;
using Invoicing.Operations;
using Microsoft.Extensions.Logging;
using static Invoicing.Models.Invoice;

namespace Invoicing.Workflows;

/// <summary>
/// Workflow for creating invoices following the DDD pattern
/// Pipeline: Created -> Calculate -> Return Event
/// 
/// VAT Calculation:
/// - Discount is distributed proportionally across lines
/// - VAT is calculated on discounted line amounts
/// - NO VAT on shipping cost
/// </summary>
public class CreateInvoiceWorkflow
{
    private readonly CalculateInvoiceOperation _calculateOperation;
    private readonly ILogger<CreateInvoiceWorkflow> _logger;

    public CreateInvoiceWorkflow(
        CalculateInvoiceOperation calculateOperation,
        ILogger<CreateInvoiceWorkflow> logger)
    {
        _calculateOperation = calculateOperation;
        _logger = logger;
    }

    public async Task<IInvoiceGeneratedEvent> ExecuteAsync(
        CreateInvoiceCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting invoice creation for Shipment: {ShipmentId}", command.ShipmentId);
            _logger.LogInformation("  Subtotal: {Subtotal}, Discount: {Discount}, TotalAfterDiscount: {TotalAfterDiscount}",
                command.Subtotal, command.DiscountAmount, command.TotalAfterDiscount);
            _logger.LogInformation("  ShippingCost: {ShippingCost}, TotalWithShipping: {TotalWithShipping}",
                command.ShippingCost, command.TotalWithShipping);

            // Calculate subtotal from lines (for proportional discount distribution)
            var subtotalFromLines = command.Lines.Sum(l => l.Quantity * l.UnitPrice);
            if (subtotalFromLines == 0) subtotalFromLines = 1; // Prevent division by zero

            // Create invoice lines with proportional discount distribution
            var invoiceLines = new List<InvoiceLine>();
            foreach (var lineInput in command.Lines)
            {
                var lineNetInitial = lineInput.Quantity * lineInput.UnitPrice;
                var lineShare = lineNetInitial / subtotalFromLines;
                
                var invoiceLine = InvoiceLine.CreateWithDiscount(
                    lineInput.Name,
                    lineInput.Description,
                    lineInput.Category,
                    lineInput.Quantity,
                    lineInput.UnitPrice,
                    lineShare,
                    command.DiscountAmount);

                invoiceLines.Add(invoiceLine);

                _logger.LogInformation("  Line: {Name}, Net: {Net}, Share: {Share:P2}, Discount: {Disc}, AfterDisc: {After}, VAT({Rate}%): {Vat}, Total: {Total}",
                    lineInput.Name,
                    invoiceLine.LineNetInitial.Value,
                    lineShare,
                    invoiceLine.LineDiscount.Value,
                    invoiceLine.LineNetAfterDiscount.Value,
                    invoiceLine.VatRate.Percentage,
                    invoiceLine.VatAmount.Value,
                    invoiceLine.LineTotalWithVat.Value);
            }

            // Calculate totals - NO VAT on shipping
            var netAfterDiscount = invoiceLines.Sum(l => l.LineNetAfterDiscount.Value);
            var totalVat = invoiceLines.Sum(l => l.VatAmount.Value);
            var productsWithVat = netAfterDiscount + totalVat;
            var grandTotal = productsWithVat + command.ShippingCost; // Shipping WITHOUT VAT

            _logger.LogInformation("Totals: NetAfterDiscount={Net}, VAT={Vat}, ProductsWithVAT={Products}, Shipping={Ship} (no VAT), GrandTotal={Total}",
                netAfterDiscount, totalVat, productsWithVat, command.ShippingCost, grandTotal);

            // Create calculated invoice directly (bypass CreatedInvoice for simplicity)
            var invoiceId = Guid.NewGuid();
            var invoiceNumber = InvoiceNumber.Generate();
            
            // Determine display currency (default to RON)
            var displayCurrency = command.DisplayCurrency ?? Currency.Default();
            var totalInRon = grandTotal;
            var totalInEur = CurrencyConverter.ConvertRonToEur(grandTotal);
            
            _logger.LogInformation("Currency: {Currency}, TotalInRON={Ron}, TotalInEUR={Eur}",
                displayCurrency.Value, totalInRon, totalInEur);

            return new InvoiceGeneratedEvent
            {
                InvoiceId = invoiceId,
                InvoiceNumber = invoiceNumber.Value,
                ShipmentId = command.ShipmentId,
                OrderId = command.OrderId,
                UserId = command.UserId,
                SubTotal = netAfterDiscount,
                Tax = totalVat,
                TotalAmount = grandTotal,
                GeneratedAt = DateTime.UtcNow,
                Currency = displayCurrency.Value,
                TotalInRon = totalInRon,
                TotalInEur = totalInEur
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating invoice for Shipment: {ShipmentId}", command.ShipmentId);
            return new InvoiceGenerationFailedEvent
            {
                ShipmentId = command.ShipmentId,
                OrderId = command.OrderId,
                Reasons = new[] { ex.Message }
            };
        }
    }
}

