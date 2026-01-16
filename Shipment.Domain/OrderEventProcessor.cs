using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharedKernel;
using SharedKernel.ServiceBus;
using Shipment.Infrastructure.Persistence;

namespace Shipment.Domain;

/// <summary>
/// Background service that listens to Order events from Service Bus and processes shipments
/// </summary>
public class OrderEventProcessor : BackgroundService
{
    private readonly ServiceBusProcessor _processor;
    private readonly ServiceBusSender _shipmentSender;
    private readonly IShipmentRepository _repository;
    private readonly ILogger<OrderEventProcessor> _logger;
    
    // Track processed messages to avoid duplicate processing
    private static readonly HashSet<string> _processedMessageIds = new();
    private static readonly object _lock = new();

    public OrderEventProcessor(
        ServiceBusClientFactory clientFactory,
        IShipmentRepository repository,
        ILogger<OrderEventProcessor> logger)
    {
        _repository = repository;
        _logger = logger;

        // Create processor for the orders topic with subscription (using orders client)
        _processor = clientFactory.OrdersClient.CreateProcessor(
            topicName: TopicNames.Orders,
            subscriptionName: SubscriptionNames.OrderProcessor,
            new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false,
                MaxConcurrentCalls = 1
            });

        // Create sender for the shipments topic (using shipments client)
        _shipmentSender = clientFactory.ShipmentsClient.CreateSender(TopicNames.Shipments);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor.ProcessMessageAsync += ProcessMessageAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;

        _logger.LogInformation("========================================");
        _logger.LogInformation("Shipment Service Started");
        _logger.LogInformation("Listening on Topic: '{Topic}', Subscription: '{Subscription}'", TopicNames.Orders, SubscriptionNames.OrderProcessor);
        _logger.LogInformation("Publishing to Topic: '{Topic}'", TopicNames.Shipments);
        _logger.LogInformation("Waiting for order events from Service Bus...");
        _logger.LogInformation("========================================");
        
        await _processor.StartProcessingAsync(stoppingToken);

        // Keep the service running
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }

        _logger.LogInformation("Stopping Order Event Processor...");
        await _processor.StopProcessingAsync(stoppingToken);
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        var messageId = args.Message.MessageId;
        
        // Check if message was already processed (deduplication)
        lock (_lock)
        {
            if (_processedMessageIds.Contains(messageId))
            {
                _logger.LogInformation("Message {MessageId} already processed, skipping (DeliveryCount: {Count})", 
                    messageId, args.Message.DeliveryCount);
                return;
            }
            _processedMessageIds.Add(messageId);
        }
        
        string messageBody = args.Message.Body.ToString();
        
        _logger.LogInformation("========================================");
        _logger.LogInformation("SERVICE BUS MESSAGE RECEIVED (NEW)");
        _logger.LogInformation("Message ID: {MessageId}", args.Message.MessageId);
        _logger.LogInformation("Message Body: {MessageBody}", messageBody);
        _logger.LogInformation("========================================");

        try
        {
            var orderEvent = JsonSerializer.Deserialize<OrderPlacedEventDto>(messageBody, JsonSerializerOptionsProvider.Default);

            if (orderEvent == null)
            {
                _logger.LogWarning("Failed to deserialize order event - message body is null or invalid");
                await args.CompleteMessageAsync(args.Message);
                return;
            }

            _logger.LogInformation("Order Event Parsed Successfully:");
            _logger.LogInformation("  - Order ID: {OrderId}", orderEvent.OrderId);
            _logger.LogInformation("  - User ID: {UserId}", orderEvent.UserId);
            _logger.LogInformation("  - Total Price: {TotalPrice:C}", orderEvent.TotalPrice);
            _logger.LogInformation("  - Lines Count: {LinesCount}", orderEvent.Lines.Count);
            _logger.LogInformation("  - Occurred At: {OccurredAt}", orderEvent.OccurredAt);

            // Create shipment entity
            var shipmentId = Guid.NewGuid();
            var trackingNumber = $"TRACK-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

            var shipmentEntity = new ShipmentEntity
            {
                ShipmentId = shipmentId,
                OrderId = orderEvent.OrderId,
                UserId = orderEvent.UserId,
                TotalPrice = orderEvent.TotalPrice,
                TrackingNumber = trackingNumber,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            var lineEntities = orderEvent.Lines.Select(l => new ShipmentLineEntity
            {
                ShipmentLineId = Guid.NewGuid(),
                ShipmentId = shipmentId,
                Name = l.Name,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                LineTotal = l.LineTotal
            }).ToList();

            // Save to database
            await _repository.SaveShipmentAsync(shipmentEntity, lineEntities, args.CancellationToken);

            _logger.LogInformation("========================================");
            _logger.LogInformation("SHIPMENT CREATED SUCCESSFULLY");
            _logger.LogInformation("  ShipmentId: {ShipmentId}", shipmentId);
            _logger.LogInformation("  TrackingNumber: {TrackingNumber}", trackingNumber);
            _logger.LogInformation("  OrderId: {OrderId}", orderEvent.OrderId);
            _logger.LogInformation("========================================");

            // Publish ShipmentCreatedEvent to shipments topic for Invoicing
            var shipmentCreatedEvent = new ShipmentCreatedEvent
            {
                ShipmentId = shipmentId,
                OrderId = orderEvent.OrderId,
                UserId = orderEvent.UserId,
                TrackingNumber = trackingNumber,
                TotalPrice = orderEvent.TotalPrice,
                Lines = orderEvent.Lines.Select(l => new ShipmentLineItem
                {
                    Name = l.Name,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice,
                    LineTotal = l.LineTotal
                }).ToList()
            };

            var eventJson = JsonSerializer.Serialize(shipmentCreatedEvent, JsonSerializerOptionsProvider.Default);
            var message = new ServiceBusMessage(eventJson)
            {
                ContentType = "application/json",
                Subject = nameof(ShipmentCreatedEvent),
                MessageId = shipmentCreatedEvent.EventId.ToString()
            };

            await _shipmentSender.SendMessageAsync(message, args.CancellationToken);
            
            _logger.LogInformation("========================================");
            _logger.LogInformation("SHIPMENT EVENT PUBLISHED TO INVOICING");
            _logger.LogInformation("  Topic: {Topic}", TopicNames.Shipments);
            _logger.LogInformation("  EventId: {EventId}", shipmentCreatedEvent.EventId);
            _logger.LogInformation("========================================");

            // Complete the message - commented out for testing (message stays in queue)
            // await args.CompleteMessageAsync(args.Message);
            // _logger.LogInformation("Message completed and removed from queue");
            _logger.LogInformation("Message processed (NOT removed from queue for testing)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order event: {ErrorMessage}", ex.Message);
            await args.AbandonMessageAsync(args.Message);
            _logger.LogWarning("Message abandoned - will be retried");
        }
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Error in Service Bus processor: {ErrorSource}", args.ErrorSource);
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _shipmentSender.DisposeAsync();
        await _processor.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }
}

/// <summary>
/// DTO for deserializing order placed events from Service Bus
/// </summary>
public class OrderPlacedEventDto
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public decimal TotalPrice { get; set; }
    public List<OrderLineDto> Lines { get; set; } = new();
    public DateTime OccurredAt { get; set; }
}

public class OrderLineDto
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
