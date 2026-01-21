using Invoicing.Events;
using Invoicing.Models;
using Invoicing.Operations;
using SharedKernel.Invoicing;
using Microsoft.Extensions.Logging;
using static Invoicing.Models.Invoice;

namespace Invoicing.Workflows;

/// <summary>
/// Workflow for creating invoices following the DDD pattern
/// Pipeline: Created -> Calculate (VAT) -> Persist -> Publish
/// 
/// VAT Calculation:
/// - Discount is distributed proportionally across lines
/// - VAT is calculated on discounted line amounts
/// - NO VAT on shipping cost
/// </summary>
public class CreateInvoiceWorkflow
{
    private readonly IInvoiceRepository _repository;
    private readonly ILogger<CreateInvoiceWorkflow> _logger;

    public CreateInvoiceWorkflow(
        IInvoiceRepository repository,
        ILogger<CreateInvoiceWorkflow> logger)
    {
        _repository = repository;
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

            // Step 1: Create invoice lines with proportional discount distribution
            var invoiceLines = CreateInvoiceLines(command);

            // Step 2: Create initial invoice state
            IInvoice invoice = CreateInvoiceFromCommand(command, invoiceLines);
            _logger.LogInformation("Invoice state: Created");

            // Step 3: Calculate VAT using operation (SYNC - pure transformation)
            var calculateVatOp = new CalculateVatOperation();
            var displayCurrency = command.DisplayCurrency ?? Currency.Default();
            invoice = calculateVatOp.Transform(invoice, displayCurrency);
            _logger.LogInformation("Invoice state: {State}", invoice.CurrentState);

            if (invoice is not CalculatedInvoice calculated)
            {
                // Return event using ToEvent() pattern - will return failed event for non-calculated states
                return invoice.ToEvent();
            }

            // Step 4: Persist to database
            var saveData = MapToSaveData(calculated);
            await _repository.SaveInvoiceAsync(saveData, cancellationToken);
            
            var persisted = new PersistedInvoice(calculated, DateTime.UtcNow);
            _logger.LogInformation("Invoice state: Persisted (InvoiceId: {InvoiceId})", persisted.InvoiceId);

            // Step 5: Return success event using ToEvent() (Lab pattern)
            _logger.LogInformation("Invoice state: Published");
            return persisted.ToEvent();
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

    private List<InvoiceLine> CreateInvoiceLines(CreateInvoiceCommand command)
    {
        // Calculate subtotal from lines (for proportional discount distribution)
        var subtotalFromLines = command.Lines.Sum(l => l.Quantity * l.UnitPrice);
        if (subtotalFromLines == 0) subtotalFromLines = 1; // Prevent division by zero

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

    private static InvoiceSaveData MapToSaveData(CalculatedInvoice calculated)
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
            Status = "Pending",
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

