using Microsoft.Extensions.Logging;
using SharedKernel;
using SharedKernel.Messaging;
using SharedKernel.ServiceBus;
using Invoicing.Models;
using Invoicing.Workflows;
using Invoicing.Events;
using Invoicing.Operations;
using SharedKernel.Invoicing;

namespace Invoicing.Handlers;

/// <summary>
/// Handler for ShipmentStateChangedEvent - creates/updates invoices based on shipment state
/// Uses AbstractEventHandler pattern from Lab
/// </summary>
public class ShipmentStateChangedHandler : AbstractEventHandler<ShipmentStateChangedEvent>
{
    private readonly CreateInvoiceWorkflow _workflow;
    private readonly IInvoiceRepository _repository;
    private readonly IEventHistoryService _eventHistory;
    private readonly IEventBus _eventBus;
    private readonly ILogger<ShipmentStateChangedHandler> _logger;

    public override string[] EventTypes => new[] { "ShipmentStateChanged", "Scheduled", "Priority", "Cancelled", "Returned" };

    public ShipmentStateChangedHandler(
        CreateInvoiceWorkflow workflow,
        IInvoiceRepository repository,
        IEventHistoryService eventHistory,
        IEventBus eventBus,
        ILogger<ShipmentStateChangedHandler> logger)
    {
        _workflow = workflow;
        _repository = repository;
        _eventHistory = eventHistory;
        _eventBus = eventBus;
        _logger = logger;
    }

    protected override async Task<EventProcessingResult> OnHandleAsync(ShipmentStateChangedEvent shipmentEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Shipment Event Received:");
        _logger.LogInformation("  Shipment ID: {ShipmentId}", shipmentEvent.ShipmentId);
        _logger.LogInformation("  Order ID: {OrderId}", shipmentEvent.OrderId);
        _logger.LogInformation("  State: {State}", shipmentEvent.ShipmentState);
        _logger.LogInformation("  Tracking: {TrackingNumber}", shipmentEvent.TrackingNumber);

        try
        {
            // Handle based on shipment state
            var result = shipmentEvent.ShipmentState switch
            {
                "Scheduled" or "Priority" => await HandleScheduledAsync(shipmentEvent, cancellationToken),
                "Cancelled" => await HandleCancelledAsync(shipmentEvent, cancellationToken),
                "Returned" => await HandleReturnedAsync(shipmentEvent, cancellationToken),
                _ => HandleUnknownState(shipmentEvent.ShipmentState)
            };

            // Save event to CSV history
            await _eventHistory.SaveEventAsync(
                shipmentEvent,
                eventType: $"ShipmentStateChanged:{shipmentEvent.ShipmentState}",
                source: TopicNames.Shipments,
                orderId: shipmentEvent.OrderId.ToString(),
                status: result.Success ? "Processed" : $"Failed: {result.ErrorMessage}"
            );
            _logger.LogInformation("Event saved to CSV history");

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling shipment event");
            return EventProcessingResult.Failed(ex);
        }
    }

    /// <summary>
    /// Data calculated once and reused for save, publish, and print operations
    /// </summary>
    private sealed class CalculatedInvoiceData
    {
        public required List<InvoiceLineSaveData> Lines { get; init; }
        public required List<LineItemDto> LineItems { get; init; }
        public decimal SubtotalFromLines { get; init; }
        public decimal TotalNetAfterDiscount { get; init; }
        public decimal TotalVat { get; init; }
        public decimal ProductsWithVat { get; init; }
        public decimal GrandTotal { get; init; }
        public decimal GrandTotalEur { get; init; }
        public required string PaymentStatus { get; init; }
    }

    /// <summary>
    /// Calculate all invoice data once from shipment event
    /// </summary>
    private CalculatedInvoiceData CalculateInvoiceData(ShipmentStateChangedEvent shipment)
    {
        var subtotalFromLines = shipment.Lines.Sum(l => l.Quantity * l.UnitPrice);
        if (subtotalFromLines == 0) subtotalFromLines = 1; // Prevent division by zero
        
        decimal totalNetAfterDiscount = 0;
        decimal totalVat = 0;
        
        var lineSaveData = new List<InvoiceLineSaveData>();
        var lineItems = new List<LineItemDto>();
        
        foreach (var l in shipment.Lines)
        {
            var category = ProductCategoryExtensions.ParseCategory(l.Category);
            var vatRate = VatRate.ForCategory(category);
            
            // Calculate proportional discount
            var lineNetInitial = l.Quantity * l.UnitPrice;
            var lineShare = lineNetInitial / subtotalFromLines;
            var lineDiscount = Math.Round(shipment.DiscountAmount * lineShare, 2);
            var lineNetAfterDiscount = Math.Max(0, lineNetInitial - lineDiscount);
            
            // VAT on discounted amount
            var lineVat = vatRate.CalculateVat(lineNetAfterDiscount);
            var lineTotal = lineNetAfterDiscount + lineVat;
            
            totalNetAfterDiscount += lineNetAfterDiscount;
            totalVat += lineVat;
            
            // For database save
            lineSaveData.Add(new InvoiceLineSaveData
            {
                InvoiceLineId = Guid.NewGuid(),
                Name = l.Name,
                Description = l.Description,
                Category = l.Category,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                LineTotal = lineTotal,
                VatRate = vatRate.Value,
                VatAmount = lineVat
            });
            
            // For event publishing
            lineItems.Add(new LineItemDto
            {
                Name = l.Name,
                Description = l.Description ?? string.Empty,
                Category = l.Category ?? string.Empty,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                LineTotal = lineTotal
            });
        }
        
        // Grand total = products with VAT + shipping (NO VAT on shipping)
        var productsWithVat = totalNetAfterDiscount + totalVat;
        var grandTotal = productsWithVat + shipment.ShippingCost;
        var grandTotalEur = Math.Round(grandTotal * 0.20m, 2); // Fixed rate: 1 RON = 0.20 EUR

        // Determine payment status based on payment method
        var paymentStatus = shipment.PaymentMethod.Equals("CardOnline", StringComparison.OrdinalIgnoreCase) 
            ? "Authorized" 
            : "Pending";

        return new CalculatedInvoiceData
        {
            Lines = lineSaveData,
            LineItems = lineItems,
            SubtotalFromLines = subtotalFromLines,
            TotalNetAfterDiscount = totalNetAfterDiscount,
            TotalVat = totalVat,
            ProductsWithVat = productsWithVat,
            GrandTotal = grandTotal,
            GrandTotalEur = grandTotalEur,
            PaymentStatus = paymentStatus
        };
    }

    private async Task<EventProcessingResult> HandleScheduledAsync(ShipmentStateChangedEvent shipmentEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling Shipment Scheduled - Creating Invoice");
        _logger.LogInformation("  Premium Subscription: {Premium}, Subtotal: {Subtotal}, Discount: {Discount}, ShippingCost: {ShippingCost}",
            shipmentEvent.PremiumSubscription, shipmentEvent.Subtotal, shipmentEvent.DiscountAmount, shipmentEvent.ShippingCost);

        // Create line inputs for invoice calculation
        var lineInputs = shipmentEvent.Lines.Select(l => new InvoiceLineInput(
            l.Name,
            l.Description ?? "",
            l.Category ?? "",
            l.Quantity,
            l.UnitPrice
        )).ToList().AsReadOnly();

        // Create invoice command with discount info
        var command = new CreateInvoiceCommand(
            shipmentEvent.ShipmentId,
            shipmentEvent.OrderId,
            shipmentEvent.UserId,
            shipmentEvent.TrackingNumber,
            shipmentEvent.PremiumSubscription,
            shipmentEvent.Subtotal,
            shipmentEvent.DiscountAmount,
            shipmentEvent.TotalAfterDiscount,
            shipmentEvent.ShippingCost,
            shipmentEvent.TotalWithShipping,
            lineInputs,
            shipmentEvent.OccurredAt);

        // Execute workflow
        var result = await _workflow.ExecuteAsync(command, cancellationToken);

        // Handle result based on event type
        return result switch
        {
            InvoiceGeneratedEvent success => await HandleSuccessAsync(success, shipmentEvent, cancellationToken),
            InvoiceGenerationFailedEvent failure => HandleFailureAsync(failure),
            _ => EventProcessingResult.Failed("Unknown workflow result")
        };
    }

    private async Task<EventProcessingResult> HandleSuccessAsync(
        InvoiceGeneratedEvent success,
        ShipmentStateChangedEvent shipmentEvent,
        CancellationToken cancellationToken)
    {
        // Calculate all data ONCE
        var calculatedData = CalculateInvoiceData(shipmentEvent);
        
        // Save to database
        await SaveInvoiceToDatabase(shipmentEvent, success, calculatedData, cancellationToken);
        
        // Print invoice
        PrintInvoice(shipmentEvent, success, calculatedData);
        
        // Publish to invoices topic
        await PublishInvoiceCreatedEvent(shipmentEvent, success, calculatedData, cancellationToken);

        _logger.LogInformation("========================================");
        _logger.LogInformation("INVOICE CREATED SUCCESSFULLY");
        _logger.LogInformation("  Invoice ID: {InvoiceId}", success.InvoiceId);
        _logger.LogInformation("  Invoice Number: {InvoiceNumber}", success.InvoiceNumber);
        _logger.LogInformation("========================================");

        return EventProcessingResult.Succeeded();
    }

    private EventProcessingResult HandleFailureAsync(InvoiceGenerationFailedEvent failure)
    {
        _logger.LogWarning("========================================");
        _logger.LogWarning("INVOICE CREATION FAILED");
        _logger.LogWarning("  Shipment ID: {ShipmentId}", failure.ShipmentId);
        _logger.LogWarning("  Reasons: {Reasons}", string.Join(", ", failure.Reasons.ToArray()));
        _logger.LogWarning("========================================");

        return EventProcessingResult.Failed(string.Join(", ", failure.Reasons));
    }

    private async Task<EventProcessingResult> HandleCancelledAsync(ShipmentStateChangedEvent shipmentEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling Shipment Cancelled - Cancelling Invoice for OrderId: {OrderId}", shipmentEvent.OrderId);

        // Update invoice status to Cancelled
        await _repository.UpdateStatusByOrderIdAsync(shipmentEvent.OrderId, "Cancelled", cancellationToken);

        _logger.LogInformation("========================================");
        _logger.LogInformation("INVOICE CANCELLED");
        _logger.LogInformation("  Order ID: {OrderId}", shipmentEvent.OrderId);
        _logger.LogInformation("  Reason: {Reason}", shipmentEvent.Reason ?? "Order cancelled");
        _logger.LogInformation("========================================");

        return EventProcessingResult.Succeeded();
    }

    private async Task<EventProcessingResult> HandleReturnedAsync(ShipmentStateChangedEvent shipmentEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling Shipment Returned - Creating Credit Note for OrderId: {OrderId}", shipmentEvent.OrderId);

        // Update invoice status to Returned (credit note issued)
        await _repository.UpdateStatusByOrderIdAsync(shipmentEvent.OrderId, "CreditNoteIssued", cancellationToken);

        _logger.LogInformation("========================================");
        _logger.LogInformation("CREDIT NOTE ISSUED");
        _logger.LogInformation("  Order ID: {OrderId}", shipmentEvent.OrderId);
        _logger.LogInformation("  Reason: {Reason}", shipmentEvent.Reason ?? "Order returned");
        _logger.LogInformation("========================================");

        return EventProcessingResult.Succeeded();
    }

    private EventProcessingResult HandleUnknownState(string state)
    {
        _logger.LogWarning("Unknown shipment state: {State}", state);
        return EventProcessingResult.Succeeded(); // Don't fail for unknown states
    }

    private async Task SaveInvoiceToDatabase(
        ShipmentStateChangedEvent shipment, 
        InvoiceGeneratedEvent invoice, 
        CalculatedInvoiceData data, 
        CancellationToken cancellationToken)
    {
        var invoiceSaveData = new InvoiceSaveData
        {
            InvoiceId = invoice.InvoiceId,
            InvoiceNumber = invoice.InvoiceNumber,
            ShipmentId = invoice.ShipmentId,
            OrderId = invoice.OrderId,
            UserId = shipment.UserId,
            TrackingNumber = shipment.TrackingNumber,
            SubTotal = data.TotalNetAfterDiscount,
            Tax = data.TotalVat,
            TotalAmount = data.GrandTotal,
            Status = data.PaymentStatus,
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Lines = data.Lines
        };

        await _repository.SaveInvoiceAsync(invoiceSaveData, cancellationToken);
        _logger.LogInformation("Invoice saved: SubTotal={SubTotal}, Discount={Discount}, VAT={Vat}, Shipping={Ship} (no VAT), Total={Total}, PaymentStatus={PaymentStatus}", 
            data.TotalNetAfterDiscount, shipment.DiscountAmount, data.TotalVat, shipment.ShippingCost, data.GrandTotal, data.PaymentStatus);
    }

    private async Task PublishInvoiceCreatedEvent(
        ShipmentStateChangedEvent shipment, 
        InvoiceGeneratedEvent invoice, 
        CalculatedInvoiceData data,
        CancellationToken cancellationToken)
    {
        var invoiceCreatedEvent = new InvoiceCreatedEvent
        {
            InvoiceId = invoice.InvoiceId,
            InvoiceNumber = invoice.InvoiceNumber,
            ShipmentId = invoice.ShipmentId,
            OrderId = invoice.OrderId,
            UserId = shipment.UserId,
            TrackingNumber = shipment.TrackingNumber,
            SubTotal = data.TotalNetAfterDiscount,
            Tax = data.TotalVat,
            TotalAmount = data.GrandTotal,
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Lines = data.LineItems,
            Currency = "RON",
            TotalInRon = data.GrandTotal,
            TotalInEur = data.GrandTotalEur
        };

        // Publish using IEventBus 
        await _eventBus.PublishAsync(TopicNames.Invoices, invoiceCreatedEvent, cancellationToken);
        
        _logger.LogInformation("========================================");
        _logger.LogInformation("INVOICE EVENT PUBLISHED");
        _logger.LogInformation("  Topic: {Topic}", TopicNames.Invoices);
        _logger.LogInformation("  EventId: {EventId}", invoiceCreatedEvent.EventId);
        _logger.LogInformation("  SubTotal: {SubTotal}, VAT: {Vat}, Total: {Total}", data.TotalNetAfterDiscount, data.TotalVat, data.GrandTotal);
        _logger.LogInformation("========================================");
    }

    private void PrintInvoice(ShipmentStateChangedEvent shipment, InvoiceGeneratedEvent invoice, CalculatedInvoiceData data)
    {
        var customerType = shipment.PremiumSubscription ? "PREMIUM (Free Shipping)" : "REGULAR";
        var hasDiscount = shipment.DiscountAmount > 0;

        _logger.LogInformation("");
        _logger.LogInformation("╔════════════════════════════════════════════════════════════════════════════════════════╗");
        _logger.LogInformation("║                                        FACTURA                                         ║");
        _logger.LogInformation("╠════════════════════════════════════════════════════════════════════════════════════════╣");
        _logger.LogInformation("║  Invoice Number: {InvoiceNumber,-70} ║", invoice.InvoiceNumber);
        _logger.LogInformation("║  Invoice ID:     {InvoiceId,-70} ║", invoice.InvoiceId);
        _logger.LogInformation("║  Date:           {Date,-70} ║", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
        _logger.LogInformation("║  Due Date:       {DueDate,-70} ║", DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd"));
        _logger.LogInformation("╠════════════════════════════════════════════════════════════════════════════════════════╣");
        _logger.LogInformation("║  ORDER DETAILS                                                                         ║");
        _logger.LogInformation("║  Order ID:       {OrderId,-70} ║", invoice.OrderId);
        _logger.LogInformation("║  Shipment ID:    {ShipmentId,-70} ║", invoice.ShipmentId);
        _logger.LogInformation("║  Tracking:       {Tracking,-70} ║", shipment.TrackingNumber);
        _logger.LogInformation("║  Customer ID:    {UserId,-70} ║", shipment.UserId);
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
        
        // Use pre-calculated line data for printing
        for (int i = 0; i < shipment.Lines.Count; i++)
        {
            var line = shipment.Lines[i];
            var lineData = data.Lines[i];
            
            var lineNetInitial = line.Quantity * line.UnitPrice;
            var lineShare = lineNetInitial / data.SubtotalFromLines;
            var lineDiscount = Math.Round(shipment.DiscountAmount * lineShare, 2);
            var lineNetAfterDisc = Math.Max(0, lineNetInitial - lineDiscount);
            var vatPercentage = (int)(lineData.VatRate * 100);
            
            var name = line.Name.Length > 18 ? line.Name[..18] : line.Name;
            
            if (hasDiscount)
            {
                _logger.LogInformation("║  {Name,-18} {Qty,3}  {Price,9:N2}  {Net,9:N2}  -{Disc,7:N2}  {After,9:N2}  {VatPct,2}%  {Vat,7:N2}  {Total,8:N2}║", 
                    name, line.Quantity, line.UnitPrice, lineNetInitial, lineDiscount, lineNetAfterDisc, vatPercentage, lineData.VatAmount, lineData.LineTotal);
            }
            else
            {
                _logger.LogInformation("║  {Name,-18} {Qty,3}   {Price,10:N2}   {Net,10:N2}    {VatPct,2}%   {Vat,8:N2}   {Total,10:N2}  ║", 
                    name, line.Quantity, line.UnitPrice, lineNetInitial, vatPercentage, lineData.VatAmount, lineData.LineTotal);
            }
        }
        
        _logger.LogInformation("╠════════════════════════════════════════════════════════════════════════════════════════╣");
        _logger.LogInformation("║                                                        Subtotal (Net):      {Net,10:N2} ║", data.SubtotalFromLines);
        
        if (hasDiscount)
        {
            _logger.LogInformation("║                                                        Discount:          -{Disc,10:N2} ║", shipment.DiscountAmount);
            _logger.LogInformation("║                                                        After Discount:     {After,10:N2} ║", data.TotalNetAfterDiscount);
        }
        
        _logger.LogInformation("║                                                        VAT (on products):   {Vat,10:N2} ║", data.TotalVat);
        _logger.LogInformation("║                                                        Products with VAT:   {Prod,10:N2} ║", data.ProductsWithVat);
        
        if (!shipment.PremiumSubscription && shipment.ShippingCost > 0)
        {
            _logger.LogInformation("║                                                        Shipping (no VAT):   {Ship,10:N2} ║", shipment.ShippingCost);
        }
        else
        {
            _logger.LogInformation("║                                                        Shipping:                  FREE ║");
        }
        
        _logger.LogInformation("║                                                        ─────────────────────────────── ║");
        _logger.LogInformation("║                                                        GRAND TOTAL (RON):   {Total,10:N2} ║", data.GrandTotal);
        _logger.LogInformation("║                                                        GRAND TOTAL (EUR):   {TotalEur,10:N2} ║", data.GrandTotalEur);
        _logger.LogInformation("║                                                        Exchange Rate: 1 RON = 0.20 EUR  ║");
        _logger.LogInformation("╚════════════════════════════════════════════════════════════════════════════════════════╝");
        _logger.LogInformation("");
    }
}
