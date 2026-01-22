using Invoicing.Events;
using Invoicing.Models;
using Invoicing.Operations;
using Microsoft.Extensions.Logging;
using static Invoicing.Models.Invoice;

namespace Invoicing.Workflows;

/// <summary>
/// Workflow for creating invoices following the DDD pattern
/// Pipeline: Created -> VatCalculated -> Calculated -> Persisted -> Published
/// 
/// VAT Calculation:
/// - Discount is distributed proportionally across lines
/// - VAT is calculated on discounted line amounts
/// - NO VAT on shipping cost
/// </summary>
public class CreateInvoiceWorkflow
{
    private readonly PersistInvoiceOperation _persistOperation;
    private readonly PublishInvoiceOperation _publishOperation;
    private readonly ILogger<CreateInvoiceWorkflow> _logger;

    public CreateInvoiceWorkflow(
        PersistInvoiceOperation persistOperation,
        PublishInvoiceOperation publishOperation,
        ILogger<CreateInvoiceWorkflow> logger)
    {
        _persistOperation = persistOperation;
        _publishOperation = publishOperation;
        _logger = logger;
    }

    public async Task<IInvoiceWorkflowResult> ExecuteAsync(
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
            _logger.LogInformation("  PaymentStatus: {PaymentStatus}", command.PaymentStatus);

            // Step 1: Create invoice lines with proportional discount distribution
            //calc fiecare produs cu voucher-ul si tva-ul aplicat
            var invoiceLines = CreateInvoiceLines(command);

            // Step 2: Create initial invoice state
            IInvoice invoice = CreateInvoiceFromCommand(command, invoiceLines);
            _logger.LogInformation("Invoice state: Created");

            // Step 3: Calculate VAT using operation
            // CreatedInvoice -> VatCalculatedInvoice
            //calculeaza subtotalul la toate produsese si toate tva-urile adunate
            invoice = new CalculateVatOperation().Transform(invoice);
            _logger.LogInformation("Invoice state: {State}", invoice.CurrentState);

            // Step 4: Finalize invoice - generate invoice number, due date
            // VatCalculatedInvoice -> CalculatedInvoice
            //total cu shipping si data scadenta (premium) si guid
            invoice = new FinalizeInvoiceOperation().Transform(invoice);
            _logger.LogInformation("Invoice state: {State}", invoice.CurrentState);

            if (invoice is not CalculatedInvoice calculated)
            {
                return invoice.ToEvent();
            }

            // Step 5: Persist to database (CalculatedInvoice -> PersistedInvoice)
            var persisted = await _persistOperation.ExecuteAsync(calculated, command.PaymentStatus, cancellationToken) as PersistedInvoice;
            if (persisted == null)
            {
                return new InvoiceCreatedFailedEvent { Reasons = new[] { "Failed to persist invoice" } };
            }
            _logger.LogInformation("Invoice state: Persisted (InvoiceId: {InvoiceId})", persisted.InvoiceId);

            // Step 6: Publish to Service Bus (PersistedInvoice -> PublishedInvoice)
            var published = await _publishOperation.ExecuteAsync(persisted, command.PaymentStatus, cancellationToken) as PublishedInvoice;
            if (published == null)
            {
                return new InvoiceCreatedFailedEvent { Reasons = new[] { "Failed to publish invoice" } };
            }
            _logger.LogInformation("Invoice state: Published");

            // Step 7: Print invoice
            PrintInvoice(command, calculated);

            // Step 8: Return success event
            return published.ToEvent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating invoice for Shipment: {ShipmentId}", command.ShipmentId);
            return new InvoiceCreatedFailedEvent
            {
                ShipmentId = command.ShipmentId,
                OrderId = command.OrderId,
                Reasons = new[] { ex.Message }
            };
        }
    }

    private List<InvoiceLine> CreateInvoiceLines(CreateInvoiceCommand command)
    {
        var subtotalFromLines = command.Lines.Sum(l => l.Quantity * l.UnitPrice);
        if (subtotalFromLines == 0) subtotalFromLines = 1;

        var invoiceLines = new List<InvoiceLine>();
        foreach (var lineInput in command.Lines)
        {
            var lineNetInitial = lineInput.Quantity * lineInput.UnitPrice;
            var lineShare = lineNetInitial / subtotalFromLines;
            //calc tva
            var invoiceLine = InvoiceLine.CreateWithDiscount(
                lineInput.Name,
                lineInput.Description,
                lineInput.Category,
                lineInput.Quantity,
                lineInput.UnitPrice,
                lineShare,
                command.DiscountAmount);

            invoiceLines.Add(invoiceLine);

            _logger.LogInformation("  Line: {Name}, Net: {Net}, Discount: {Disc}, VAT({Rate}%): {Vat}",
                lineInput.Name,
                invoiceLine.LineNetInitial.Value,
                invoiceLine.LineDiscount.Value,
                invoiceLine.VatRate.Percentage,
                invoiceLine.VatAmount.Value);
        }

        return invoiceLines;
    }

    private CreatedInvoice CreateInvoiceFromCommand(CreateInvoiceCommand command, List<InvoiceLine> lines)
    {
        return CreatedInvoice.CreateFromEvent(
            command.ShipmentId,
            command.OrderId,
            command.UserId,
            command.TrackingNumber,
            command.PremiumSubscription,
            command.TotalAfterDiscount,
            command.ShippingCost,
            command.TotalWithShipping,
            lines.AsReadOnly(),
            DateTime.UtcNow);
    }


    private void PrintInvoice(CreateInvoiceCommand command, CalculatedInvoice invoice)
    {
        var customerType = command.PremiumSubscription ? "PREMIUM (Free Shipping)" : "REGULAR";
        var hasDiscount = command.DiscountAmount > 0;
        
        // Calculate totals for display
        var subtotalFromLines = command.Lines.Sum(l => l.Quantity * l.UnitPrice);
        var totalNetAfterDiscount = invoice.Lines.Sum(l => l.LineNetAfterDiscount.Value);
        var totalVat = invoice.Tax.Value;
        var productsWithVat = totalNetAfterDiscount + totalVat;
        var grandTotal = productsWithVat + command.ShippingCost;
        var grandTotalEur = Math.Round(grandTotal * 0.20m, 2);

        _logger.LogInformation("");
        _logger.LogInformation("╔════════════════════════════════════════════════════════════════════════════════════════╗");
        _logger.LogInformation("║                                        FACTURA                                         ║");
        _logger.LogInformation("╠════════════════════════════════════════════════════════════════════════════════════════╣");
        _logger.LogInformation("║  Invoice Number: {InvoiceNumber,-70} ║", invoice.InvoiceNumber.Value);
        _logger.LogInformation("║  Invoice ID:     {InvoiceId,-70} ║", invoice.InvoiceId);
        _logger.LogInformation("║  Date:           {Date,-70} ║", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
        _logger.LogInformation("║  Due Date:       {DueDate,-70} ║", invoice.DueDate.ToString("yyyy-MM-dd"));
        _logger.LogInformation("║  Payment Status: {PaymentStatus,-70} ║", command.PaymentStatus);
        _logger.LogInformation("╠════════════════════════════════════════════════════════════════════════════════════════╣");
        _logger.LogInformation("║  ORDER DETAILS                                                                         ║");
        _logger.LogInformation("║  Order ID:       {OrderId,-70} ║", command.OrderId);
        _logger.LogInformation("║  Shipment ID:    {ShipmentId,-70} ║", command.ShipmentId);
        _logger.LogInformation("║  Tracking:       {Tracking,-70} ║", command.TrackingNumber);
        _logger.LogInformation("║  Customer ID:    {UserId,-70} ║", command.UserId);
        _logger.LogInformation("║  Customer Type:  {CustomerType,-70} ║", customerType);
        _logger.LogInformation("╠════════════════════════════════════════════════════════════════════════════════════════╣");
        
        if (hasDiscount)
        {
            _logger.LogInformation("║  ITEMS (VAT on discounted amounts - Essential=11%, Electronics/Other=21%)              ║");
            _logger.LogInformation("║  ────────────────────────────────────────────────────────────────────────────────────── ║");
            _logger.LogInformation("║  Product              Qty   UnitPrice     Net    Discount   AfterDisc  VAT%   VAT   Total║");
        }
        else
        {
            _logger.LogInformation("║  ITEMS (VAT: Essential=11%, Electronics/Other=21%)                                      ║");
            _logger.LogInformation("║  ────────────────────────────────────────────────────────────────────────────────────── ║");
            _logger.LogInformation("║  Product              Qty    Unit Price      Net       VAT%      VAT       Total        ║");
        }
        _logger.LogInformation("║  ────────────────────────────────────────────────────────────────────────────────────── ║");
        
        // Print each line using calculated invoice lines
        foreach (var line in invoice.Lines)
        {
            var name = line.Name.Value.Length > 18 ? line.Name.Value[..18] : line.Name.Value;
            var vatPercentage = (int)line.VatRate.Percentage;
            
            if (hasDiscount)
            {
                _logger.LogInformation("║  {Name,-18} {Qty,3}  {Price,9:N2}  {Net,9:N2}  -{Disc,7:N2}  {After,9:N2}  {VatPct,2}%  {Vat,7:N2}  {Total,8:N2}║", 
                    name, 
                    line.Quantity.Value, 
                    line.UnitPrice.Value, 
                    line.LineNetInitial.Value, 
                    line.LineDiscount.Value, 
                    line.LineNetAfterDiscount.Value, 
                    vatPercentage, 
                    line.VatAmount.Value, 
                    line.LineTotalWithVat.Value);
            }
            else
            {
                _logger.LogInformation("║  {Name,-18} {Qty,3}   {Price,10:N2}   {Net,10:N2}    {VatPct,2}%   {Vat,8:N2}   {Total,10:N2}  ║", 
                    name, 
                    line.Quantity.Value, 
                    line.UnitPrice.Value, 
                    line.LineNetInitial.Value, 
                    vatPercentage, 
                    line.VatAmount.Value, 
                    line.LineTotalWithVat.Value);
            }
        }
        
        _logger.LogInformation("╠════════════════════════════════════════════════════════════════════════════════════════╣");
        _logger.LogInformation("║                                                        Subtotal (Net):      {Net,10:N2} ║", subtotalFromLines);
        
        if (hasDiscount)
        {
            _logger.LogInformation("║                                                        Discount:          -{Disc,10:N2} ║", command.DiscountAmount);
            _logger.LogInformation("║                                                        After Discount:     {After,10:N2} ║", totalNetAfterDiscount);
        }
        
        _logger.LogInformation("║                                                        VAT (on products):   {Vat,10:N2} ║", totalVat);
        _logger.LogInformation("║                                                        Products with VAT:   {Prod,10:N2} ║", productsWithVat);
        
        if (!command.PremiumSubscription && command.ShippingCost > 0)
        {
            _logger.LogInformation("║                                                        Shipping (no VAT):   {Ship,10:N2} ║", command.ShippingCost);
        }
        else
        {
            _logger.LogInformation("║                                                        Shipping:                  FREE ║");
        }
        
        _logger.LogInformation("║                                                        ─────────────────────────────── ║");
        _logger.LogInformation("║                                                        GRAND TOTAL (RON):   {Total,10:N2} ║", grandTotal);
        _logger.LogInformation("║                                                        GRAND TOTAL (EUR):   {TotalEur,10:N2} ║", grandTotalEur);
        _logger.LogInformation("║                                                        Exchange Rate: 1 RON = 0.20 EUR  ║");
        _logger.LogInformation("╚════════════════════════════════════════════════════════════════════════════════════════╝");
        _logger.LogInformation("");
    }
}
