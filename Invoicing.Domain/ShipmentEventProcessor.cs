using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharedKernel;
using SharedKernel.ServiceBus;
using Invoicing.Models;
using Invoicing.Workflows;
using Invoicing.Infrastructure.Persistence;
using static Invoicing.Events.InvoiceProcessedEvent;

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
    private readonly ILogger<ShipmentEventProcessor> _logger;

    public ShipmentEventProcessor(
        ServiceBusClientFactory clientFactory,
        CreateInvoiceWorkflow workflow,
        IInvoiceRepository repository,
        ILogger<ShipmentEventProcessor> logger)
    {
        _workflow = workflow;
        _repository = repository;
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
            var shipmentEvent = JsonSerializer.Deserialize<ShipmentCreatedEvent>(messageBody, JsonSerializerOptionsProvider.Default);

            if (shipmentEvent == null)
            {
                _logger.LogWarning("Failed to deserialize shipment event");
                await args.CompleteMessageAsync(args.Message);
                return;
            }

            _logger.LogInformation("Shipment Event Received:");
            _logger.LogInformation("  Shipment ID: {ShipmentId}", shipmentEvent.ShipmentId);
            _logger.LogInformation("  Order ID: {OrderId}", shipmentEvent.OrderId);
            _logger.LogInformation("  Tracking: {TrackingNumber}", shipmentEvent.TrackingNumber);

            // Create invoice command
            var command = new CreateInvoiceCommand(
                shipmentEvent.ShipmentId,
                shipmentEvent.OrderId,
                shipmentEvent.UserId,
                shipmentEvent.TrackingNumber,
                shipmentEvent.TotalPrice,
                shipmentEvent.Lines.Select(l => new InvoiceLine(l.Name, l.Quantity, l.UnitPrice, l.LineTotal)).ToList().AsReadOnly(),
                shipmentEvent.OccurredAt);

            // Execute workflow
            var result = await _workflow.ExecuteAsync(command, args.CancellationToken);

            // Display invoice and save to DB
            switch (result)
            {
                case InvoiceCreatedSuccessfullyEvent success:
                    // Save to database
                    await SaveInvoiceToDatabase(shipmentEvent, success, args.CancellationToken);
                    
                    // Print invoice
                    PrintInvoice(shipmentEvent, success);
                    
                    // Publish to invoices topic
                    await PublishInvoiceCreatedEvent(shipmentEvent, success, args.CancellationToken);
                    break;

                case InvoiceCreationFailedEvent failure:
                    _logger.LogWarning("========================================");
                    _logger.LogWarning("INVOICE CREATION FAILED");
                    _logger.LogWarning("  Shipment ID: {ShipmentId}", failure.ShipmentId);
                    _logger.LogWarning("  Reasons: {Reasons}", string.Join(", ", failure.Reasons));
                    _logger.LogWarning("========================================");
                    break;
            }

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

    private async Task SaveInvoiceToDatabase(ShipmentCreatedEvent shipment, InvoiceCreatedSuccessfullyEvent invoice, CancellationToken cancellationToken)
    {
        var subTotal = shipment.TotalPrice;
        var tax = Math.Round(subTotal * 0.19m, 2);
        var total = subTotal + tax;

        var invoiceEntity = new InvoiceEntity
        {
            InvoiceId = invoice.InvoiceId,
            InvoiceNumber = invoice.InvoiceNumber,
            ShipmentId = invoice.ShipmentId,
            OrderId = invoice.OrderId,
            UserId = shipment.UserId,
            TrackingNumber = shipment.TrackingNumber,
            SubTotal = subTotal,
            Tax = tax,
            TotalAmount = total,
            Status = "Pending",
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };

        var lineEntities = shipment.Lines.Select(l => new InvoiceLineEntity
        {
            InvoiceLineId = Guid.NewGuid(),
            InvoiceId = invoice.InvoiceId,
            Name = l.Name,
            Quantity = l.Quantity,
            UnitPrice = l.UnitPrice,
            LineTotal = l.LineTotal
        }).ToList();

        await _repository.SaveInvoiceAsync(invoiceEntity, lineEntities, cancellationToken);
        _logger.LogInformation("Invoice saved to database: {InvoiceId}", invoice.InvoiceId);
    }

    private async Task PublishInvoiceCreatedEvent(ShipmentCreatedEvent shipment, InvoiceCreatedSuccessfullyEvent invoice, CancellationToken cancellationToken)
    {
        var subTotal = shipment.TotalPrice;
        var tax = Math.Round(subTotal * 0.19m, 2);
        var total = subTotal + tax;

        var invoiceCreatedEvent = new InvoiceCreatedEvent
        {
            InvoiceId = invoice.InvoiceId,
            InvoiceNumber = invoice.InvoiceNumber,
            ShipmentId = invoice.ShipmentId,
            OrderId = invoice.OrderId,
            UserId = shipment.UserId,
            TrackingNumber = shipment.TrackingNumber,
            SubTotal = subTotal,
            Tax = tax,
            TotalAmount = total,
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Lines = shipment.Lines.Select(l => new InvoiceLineItem
            {
                Name = l.Name,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                LineTotal = l.LineTotal
            }).ToList()
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
        _logger.LogInformation("========================================");
    }

    private void PrintInvoice(ShipmentCreatedEvent shipment, InvoiceCreatedSuccessfullyEvent invoice)
    {
        var subTotal = shipment.TotalPrice;
        var tax = Math.Round(subTotal * 0.19m, 2);
        var total = subTotal + tax;

        _logger.LogInformation("");
        _logger.LogInformation("╔══════════════════════════════════════════════════════════════╗");
        _logger.LogInformation("║                         FACTURA                              ║");
        _logger.LogInformation("╠══════════════════════════════════════════════════════════════╣");
        _logger.LogInformation("║  Invoice Number: {InvoiceNumber,-43} ║", invoice.InvoiceNumber);
        _logger.LogInformation("║  Invoice ID:     {InvoiceId,-43} ║", invoice.InvoiceId);
        _logger.LogInformation("║  Date:           {Date,-43} ║", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
        _logger.LogInformation("║  Due Date:       {DueDate,-43} ║", DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd"));
        _logger.LogInformation("╠══════════════════════════════════════════════════════════════╣");
        _logger.LogInformation("║  ORDER DETAILS                                               ║");
        _logger.LogInformation("║  Order ID:       {OrderId,-43} ║", invoice.OrderId);
        _logger.LogInformation("║  Shipment ID:    {ShipmentId,-43} ║", invoice.ShipmentId);
        _logger.LogInformation("║  Tracking:       {Tracking,-43} ║", shipment.TrackingNumber);
        _logger.LogInformation("║  Customer ID:    {UserId,-43} ║", shipment.UserId);
        _logger.LogInformation("╠══════════════════════════════════════════════════════════════╣");
        _logger.LogInformation("║  ITEMS                                                       ║");
        _logger.LogInformation("║  ----------------------------------------------------------- ║");
        
        foreach (var line in shipment.Lines)
        {
            _logger.LogInformation("║  {Name,-20} x{Qty,3}  @{Price,10:C}  = {Total,12:C} ║", 
                line.Name.Length > 20 ? line.Name[..20] : line.Name, 
                line.Quantity, 
                line.UnitPrice, 
                line.LineTotal);
        }
        
        _logger.LogInformation("╠══════════════════════════════════════════════════════════════╣");
        _logger.LogInformation("║                                    Subtotal:   {SubTotal,12:C} ║", subTotal);
        _logger.LogInformation("║                                    TVA (19%):  {Tax,12:C} ║", tax);
        _logger.LogInformation("║                                    ─────────────────────── ║");
        _logger.LogInformation("║                                    TOTAL:      {Total,12:C} ║", total);
        _logger.LogInformation("╚══════════════════════════════════════════════════════════════╝");
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

