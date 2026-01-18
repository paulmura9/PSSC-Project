using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharedKernel;
using SharedKernel.ServiceBus;
using Invoicing.Models;
using Invoicing.Workflows;
using Invoicing.Infrastructure.Persistence;
using Invoicing.Infrastructure.Repository;
using Invoicing.Events;

namespace Invoicing;

/// <summary>
/// Background service that listens to Shipment events from Service Bus and creates invoices
/// </summary>
public class ShipmentEventProcessor : BackgroundService
{
    private readonly ServiceBusProcessor _processor;
    private readonly ServiceBusSender _invoiceSender;
    private readonly CreateInvoiceWorkflow _workflow;
    private readonly IInvoiceRepository _repository;
    private readonly IEventHistoryService _eventHistory;
    private readonly ILogger<ShipmentEventProcessor> _logger;

    public ShipmentEventProcessor(
        ServiceBusClientFactory clientFactory,
        CreateInvoiceWorkflow workflow,
        IInvoiceRepository repository,
        IEventHistoryService eventHistory,
        ILogger<ShipmentEventProcessor> logger)
    {
        _workflow = workflow;
        _repository = repository;
        _eventHistory = eventHistory;
        _logger = logger;

        // Create processor for the shipments topic with subscription
        _processor = clientFactory.ShipmentsClient.CreateProcessor(
            topicName: TopicNames.Shipments,
            subscriptionName: SubscriptionNames.ShipmentProcessor,
            new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false,
                MaxConcurrentCalls = 1
            });

        // Create sender for the invoices topic
        _invoiceSender = clientFactory.InvoicesClient.CreateSender(TopicNames.Invoices);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor.ProcessMessageAsync += ProcessMessageAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;

        _logger.LogInformation("========================================");
        _logger.LogInformation("Invoicing.Domain Service Started");
        _logger.LogInformation("Listening on Topic: '{Topic}', Subscription: '{Subscription}'", TopicNames.Shipments, SubscriptionNames.ShipmentProcessor);
        _logger.LogInformation("Publishing to Topic: '{Topic}'", TopicNames.Invoices);
        _logger.LogInformation("Waiting for shipment events from Service Bus...");
        _logger.LogInformation("========================================");
        
        await _processor.StartProcessingAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }

        _logger.LogInformation("Stopping Shipment Event Processor...");
        await _processor.StopProcessingAsync(stoppingToken);
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        string messageBody = args.Message.Body.ToString();
        
        _logger.LogInformation("========================================");
        _logger.LogInformation("SERVICE BUS MESSAGE RECEIVED");
        _logger.LogInformation("Message ID: {MessageId}", args.Message.MessageId);
        _logger.LogInformation("========================================");

        try
        {
            var shipmentEvent = JsonSerializer.Deserialize<ShipmentStateChangedEvent>(messageBody, JsonSerializerOptionsProvider.Default);

            if (shipmentEvent == null)
            {
                _logger.LogWarning("Failed to deserialize shipment event");
                await args.CompleteMessageAsync(args.Message);
                return;
            }

            _logger.LogInformation("Shipment Event Received:");
            _logger.LogInformation("  Shipment ID: {ShipmentId}", shipmentEvent.ShipmentId);
            _logger.LogInformation("  Order ID: {OrderId}", shipmentEvent.OrderId);
            _logger.LogInformation("  State: {State}", shipmentEvent.ShipmentState);
            _logger.LogInformation("  Tracking: {TrackingNumber}", shipmentEvent.TrackingNumber);

            // Handle based on shipment state
            switch (shipmentEvent.ShipmentState)
            {
                case "Scheduled":
                case "Priority":  // Premium customers get Priority status
                    await HandleShipmentScheduledAsync(shipmentEvent, args.CancellationToken);
                    break;
                case "Cancelled":
                    await HandleShipmentCancelledAsync(shipmentEvent, args.CancellationToken);
                    break;
                case "Returned":
                    await HandleShipmentReturnedAsync(shipmentEvent, args.CancellationToken);
                    break;
                default:
                    _logger.LogWarning("Unknown shipment state: {State}", shipmentEvent.ShipmentState);
                    break;
            }

            // Save event to CSV history before removing from queue
            await _eventHistory.SaveEventAsync(
                shipmentEvent,
                eventType: $"ShipmentStateChanged:{shipmentEvent.ShipmentState}",
                source: TopicNames.Shipments,
                orderId: shipmentEvent.OrderId.ToString(),
                status: "Processed"
            );
            _logger.LogInformation("Event saved to CSV history");

            // Complete the message
            await args.CompleteMessageAsync(args.Message);
            _logger.LogInformation("Message completed and removed from queue");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing shipment event: {ErrorMessage}", ex.Message);
            await args.AbandonMessageAsync(args.Message);
            _logger.LogWarning("Message abandoned - will be retried");
        }
    }

    private async Task HandleShipmentScheduledAsync(ShipmentStateChangedEvent shipmentEvent, CancellationToken cancellationToken)
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

        // Display invoice and save to DB
        switch (result)
        {
            case InvoiceGeneratedEvent success:
                // Save to database
                await SaveInvoiceToDatabase(shipmentEvent, success, cancellationToken);
                
                // Print invoice
                PrintInvoice(shipmentEvent, success);
                
                // Publish to invoices topic
                await PublishInvoiceCreatedEvent(shipmentEvent, success, cancellationToken);
                break;

            case InvoiceGenerationFailedEvent failure:
                _logger.LogWarning("========================================");
                _logger.LogWarning("INVOICE CREATION FAILED");
                _logger.LogWarning("  Shipment ID: {ShipmentId}", failure.ShipmentId);
                _logger.LogWarning("  Reasons: {Reasons}", string.Join(", ", failure.Reasons.ToArray()));
                _logger.LogWarning("========================================");
                break;
        }
    }

    private async Task HandleShipmentCancelledAsync(ShipmentStateChangedEvent shipmentEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling Shipment Cancelled - Cancelling Invoice for OrderId: {OrderId}", shipmentEvent.OrderId);

        // Update invoice status to Cancelled
        await _repository.UpdateStatusByOrderIdAsync(shipmentEvent.OrderId, "Cancelled", cancellationToken);

        _logger.LogInformation("========================================");
        _logger.LogInformation("INVOICE CANCELLED");
        _logger.LogInformation("  Order ID: {OrderId}", shipmentEvent.OrderId);
        _logger.LogInformation("  Reason: {Reason}", shipmentEvent.Reason ?? "Order cancelled");
        _logger.LogInformation("========================================");
    }

    private async Task HandleShipmentReturnedAsync(ShipmentStateChangedEvent shipmentEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling Shipment Returned - Creating Credit Note for OrderId: {OrderId}", shipmentEvent.OrderId);

        // Update invoice status to Returned (credit note issued)
        await _repository.UpdateStatusByOrderIdAsync(shipmentEvent.OrderId, "CreditNoteIssued", cancellationToken);

        _logger.LogInformation("========================================");
        _logger.LogInformation("CREDIT NOTE ISSUED");
        _logger.LogInformation("  Order ID: {OrderId}", shipmentEvent.OrderId);
        _logger.LogInformation("  Reason: {Reason}", shipmentEvent.Reason ?? "Order returned");
        _logger.LogInformation("========================================");
    }

    private async Task SaveInvoiceToDatabase(ShipmentStateChangedEvent shipment, InvoiceGeneratedEvent invoice, CancellationToken cancellationToken)
    {
        // Calculate subtotal for proportional discount distribution
        var subtotalFromLines = shipment.Lines.Sum(l => l.Quantity * l.UnitPrice);
        if (subtotalFromLines == 0) subtotalFromLines = 1; // Prevent division by zero
        
        decimal totalNetAfterDiscount = 0;
        decimal totalVat = 0;
        
        var lineEntities = shipment.Lines.Select(l => 
        {
            var category = Invoicing.Models.ProductCategoryExtensions.ParseCategory(l.Category);
            var vatRate = Invoicing.Models.VatRate.ForCategory(category);
            
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
            
            return new InvoiceLineEntity
            {
                InvoiceLineId = Guid.NewGuid(),
                InvoiceId = invoice.InvoiceId,
                Name = l.Name,
                Description = l.Description ?? string.Empty,
                Category = l.Category ?? string.Empty,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                LineTotal = lineTotal
            };
        }).ToList();
        
        // Grand total = products with VAT + shipping (NO VAT on shipping)
        var productsWithVat = totalNetAfterDiscount + totalVat;
        var grandTotal = productsWithVat + shipment.ShippingCost;

        // Determine payment status based on payment method
        // CardOnline = already paid (Authorized)
        // CashOnDelivery/CardOnDelivery = pay at delivery (Pending)
        var paymentStatus = shipment.PaymentMethod.Equals("CardOnline", StringComparison.OrdinalIgnoreCase) 
            ? "Authorized" 
            : "Pending";

        var invoiceEntity = new InvoiceEntity
        {
            InvoiceId = invoice.InvoiceId,
            InvoiceNumber = invoice.InvoiceNumber,
            ShipmentId = invoice.ShipmentId,
            OrderId = invoice.OrderId,
            UserId = shipment.UserId,
            TrackingNumber = shipment.TrackingNumber,
            SubTotal = totalNetAfterDiscount,
            Tax = totalVat,
            TotalAmount = grandTotal,
            Status = paymentStatus,
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };

        await _repository.SaveInvoiceAsync(invoiceEntity, lineEntities, cancellationToken);
        _logger.LogInformation("Invoice saved: SubTotal={SubTotal}, Discount={Discount}, VAT={Vat}, Shipping={Ship} (no VAT), Total={Total}, PaymentStatus={PaymentStatus}", 
            totalNetAfterDiscount, shipment.DiscountAmount, totalVat, shipment.ShippingCost, grandTotal, paymentStatus);
    }

    private async Task PublishInvoiceCreatedEvent(ShipmentStateChangedEvent shipment, InvoiceGeneratedEvent invoice, CancellationToken cancellationToken)
    {
        // Calculate subtotal for proportional discount distribution
        var subtotalFromLines = shipment.Lines.Sum(l => l.Quantity * l.UnitPrice);
        if (subtotalFromLines == 0) subtotalFromLines = 1;
        
        decimal totalNetAfterDiscount = 0;
        decimal totalVat = 0;
        
        var lineItems = shipment.Lines.Select(l => 
        {
            var category = Invoicing.Models.ProductCategoryExtensions.ParseCategory(l.Category);
            var vatRate = Invoicing.Models.VatRate.ForCategory(category);
            
            // Calculate proportional discount
            var lineNetInitial = l.Quantity * l.UnitPrice;
            var lineShare = lineNetInitial / subtotalFromLines;
            var lineDiscount = Math.Round(shipment.DiscountAmount * lineShare, 2);
            var lineNetAfterDisc = Math.Max(0, lineNetInitial - lineDiscount);
            
            // VAT on discounted amount
            var lineVat = vatRate.CalculateVat(lineNetAfterDisc);
            var lineTotal = lineNetAfterDisc + lineVat;
            
            totalNetAfterDiscount += lineNetAfterDisc;
            totalVat += lineVat;
            
            return new InvoiceLineItem
            {
                Name = l.Name,
                Description = l.Description ?? string.Empty,
                Category = l.Category ?? string.Empty,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                LineTotal = lineTotal
            };
        }).ToList();
        
        // Grand total = products with VAT + shipping (NO VAT on shipping)
        var productsWithVat = totalNetAfterDiscount + totalVat;
        var grandTotal = productsWithVat + shipment.ShippingCost;
        
        // Currency conversion - DB stores RON, EUR is derived
        var totalInRon = grandTotal;
        var totalInEur = Math.Round(grandTotal * 0.20m, 2); // Fixed rate: 1 RON = 0.20 EUR

        var invoiceCreatedEvent = new InvoiceCreatedEvent
        {
            InvoiceId = invoice.InvoiceId,
            InvoiceNumber = invoice.InvoiceNumber,
            ShipmentId = invoice.ShipmentId,
            OrderId = invoice.OrderId,
            UserId = shipment.UserId,
            TrackingNumber = shipment.TrackingNumber,
            SubTotal = totalNetAfterDiscount,
            Tax = totalVat,
            TotalAmount = grandTotal,
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Lines = lineItems,
            Currency = "RON",
            TotalInRon = totalInRon,
            TotalInEur = totalInEur
        };

        var eventJson = JsonSerializer.Serialize(invoiceCreatedEvent, JsonSerializerOptionsProvider.Default);
        var message = new ServiceBusMessage(eventJson)
        {
            ContentType = "application/json",
            Subject = nameof(InvoiceCreatedEvent),
            MessageId = invoiceCreatedEvent.EventId.ToString()
        };

        await _invoiceSender.SendMessageAsync(message, cancellationToken);
        
        _logger.LogInformation("========================================");
        _logger.LogInformation("INVOICE EVENT PUBLISHED");
        _logger.LogInformation("  Topic: {Topic}", TopicNames.Invoices);
        _logger.LogInformation("  EventId: {EventId}", invoiceCreatedEvent.EventId);
        _logger.LogInformation("  SubTotal: {SubTotal}, VAT: {Vat}, Total: {Total}", totalNetAfterDiscount, totalVat, grandTotal);
        _logger.LogInformation("========================================");
    }

    private void PrintInvoice(ShipmentStateChangedEvent shipment, InvoiceGeneratedEvent invoice)
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
        
        // Calculate subtotal for proportional discount distribution
        var subtotalFromLines = shipment.Lines.Sum(l => l.Quantity * l.UnitPrice);
        if (subtotalFromLines == 0) subtotalFromLines = 1;
        
        decimal totalNetAfterDiscount = 0;
        decimal totalVat = 0;
        
        foreach (var line in shipment.Lines)
        {
            var category = Invoicing.Models.ProductCategoryExtensions.ParseCategory(line.Category);
            var vatRate = Invoicing.Models.VatRate.ForCategory(category);
            
            var lineNetInitial = line.Quantity * line.UnitPrice;
            var lineShare = lineNetInitial / subtotalFromLines;
            var lineDiscount = Math.Round(shipment.DiscountAmount * lineShare, 2);
            var lineNetAfterDisc = Math.Max(0, lineNetInitial - lineDiscount);
            var lineVat = vatRate.CalculateVat(lineNetAfterDisc);
            var lineTotal = lineNetAfterDisc + lineVat;
            
            totalNetAfterDiscount += lineNetAfterDisc;
            totalVat += lineVat;
            
            var name = line.Name.Length > 18 ? line.Name[..18] : line.Name;
            
            if (hasDiscount)
            {
                _logger.LogInformation("║  {Name,-18} {Qty,3}  {Price,9:N2}  {Net,9:N2}  -{Disc,7:N2}  {After,9:N2}  {VatPct,2}%  {Vat,7:N2}  {Total,8:N2}║", 
                    name, line.Quantity, line.UnitPrice, lineNetInitial, lineDiscount, lineNetAfterDisc, vatRate.Percentage, lineVat, lineTotal);
            }
            else
            {
                _logger.LogInformation("║  {Name,-18} {Qty,3}   {Price,10:N2}   {Net,10:N2}    {VatPct,2}%   {Vat,8:N2}   {Total,10:N2}  ║", 
                    name, line.Quantity, line.UnitPrice, lineNetInitial, vatRate.Percentage, lineVat, lineTotal);
            }
        }
        
        var productsWithVat = totalNetAfterDiscount + totalVat;
        var grandTotal = productsWithVat + shipment.ShippingCost; // NO VAT on shipping
        
        _logger.LogInformation("╠════════════════════════════════════════════════════════════════════════════════════════╣");
        _logger.LogInformation("║                                                        Subtotal (Net):      {Net,10:N2} ║", subtotalFromLines);
        
        if (hasDiscount)
        {
            _logger.LogInformation("║                                                        Discount:          -{Disc,10:N2} ║", shipment.DiscountAmount);
            _logger.LogInformation("║                                                        After Discount:     {After,10:N2} ║", totalNetAfterDiscount);
        }
        
        _logger.LogInformation("║                                                        VAT (on products):   {Vat,10:N2} ║", totalVat);
        _logger.LogInformation("║                                                        Products with VAT:   {Prod,10:N2} ║", productsWithVat);
        
        if (!shipment.PremiumSubscription && shipment.ShippingCost > 0)
        {
            _logger.LogInformation("║                                                        Shipping (no VAT):   {Ship,10:N2} ║", shipment.ShippingCost);
        }
        else
        {
            _logger.LogInformation("║                                                        Shipping:                  FREE ║");
        }
        
        // Currency conversion - DB stores RON, EUR is derived (1 RON = 0.20 EUR)
        var grandTotalEur = Math.Round(grandTotal * 0.20m, 2);
        
        _logger.LogInformation("║                                                        ─────────────────────────────── ║");
        _logger.LogInformation("║                                                        GRAND TOTAL (RON):   {Total,10:N2} ║", grandTotal);
        _logger.LogInformation("║                                                        GRAND TOTAL (EUR):   {TotalEur,10:N2} ║", grandTotalEur);
        _logger.LogInformation("║                                                        Exchange Rate: 1 RON = 0.20 EUR  ║");
        _logger.LogInformation("╚════════════════════════════════════════════════════════════════════════════════════════╝");
        _logger.LogInformation("");
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Error in Service Bus processor: {ErrorSource}", args.ErrorSource);
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _invoiceSender.DisposeAsync();
        await _processor.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }
}

