using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharedKernel;
using SharedKernel.ServiceBus;
using Shipment.Domain.Operations;
using Shipment.Infrastructure.Persistence;
using IShipmentRepository = Shipment.Infrastructure.Persistence.IShipmentRepository;

namespace Shipment.Domain;

/// <summary>
/// DTO for deserializing OrderStateChangedEvent from Service Bus
/// </summary>
public class OrderStateChangedEventDto
{
    public Guid EventId { get; set; }
    public DateTime OccurredAt { get; set; }
    public string OrderStatus { get; set; } = string.Empty;
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public bool PremiumSubscription { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total { get; set; }
    public string? VoucherCode { get; set; }
    public decimal TotalPrice { get; set; } // Legacy
    public List<OrderLineEventDto> Lines { get; set; } = new();
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Reason { get; set; }
    public string PaymentMethod { get; set; } = "CashOnDelivery";
}

public class OrderLineEventDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

/// <summary>
/// Background service that listens to Order events from Service Bus and processes shipments
/// </summary>
public class OrderEventProcessor : BackgroundService
{
    private readonly ServiceBusProcessor _processor;
    private readonly ServiceBusSender _shipmentSender;
    private readonly IShipmentRepository _repository;
    private readonly IEventHistoryService _eventHistory;
    private readonly ILogger<OrderEventProcessor> _logger;
    
    private static readonly HashSet<string> ProcessedMessageIds = new();
    private static readonly object Lock = new();

    public OrderEventProcessor(
        ServiceBusClientFactory clientFactory,
        IShipmentRepository repository,
        IEventHistoryService eventHistory,
        ILogger<OrderEventProcessor> logger)
    {
        _repository = repository;
        _eventHistory = eventHistory;
        _logger = logger;

        _processor = clientFactory.OrdersClient.CreateProcessor(
            topicName: TopicNames.Orders,
            subscriptionName: SubscriptionNames.OrderProcessor,
            new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false,
                MaxConcurrentCalls = 1
            });

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
        _logger.LogInformation("Waiting for OrderStateChangedEvent from Service Bus...");
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
        lock (Lock)
        {
            if (ProcessedMessageIds.Contains(messageId))
            {
                _logger.LogInformation("Message {MessageId} already processed, skipping", messageId);
                return;
            }
            ProcessedMessageIds.Add(messageId);
        }
        
        string messageBody = args.Message.Body.ToString();
        
        _logger.LogInformation("========================================");
        _logger.LogInformation("SERVICE BUS MESSAGE RECEIVED");
        _logger.LogInformation("Message ID: {MessageId}", messageId);
        _logger.LogInformation("========================================");

        try
        {
            var orderEvent = JsonSerializer.Deserialize<OrderStateChangedEventDto>(messageBody, JsonSerializerOptionsProvider.Default);

            if (orderEvent == null)
            {
                _logger.LogWarning("Failed to deserialize order event");
                await args.CompleteMessageAsync(args.Message);
                return;
            }

            _logger.LogInformation("Order Event: OrderId={OrderId}, Status={Status}", orderEvent.OrderId, orderEvent.OrderStatus);

            // Handle based on order status
            switch (orderEvent.OrderStatus)
            {
                case "Placed":
                    await HandleOrderPlacedAsync(orderEvent, args.CancellationToken);
                    break;
                case "Cancelled":
                    await HandleOrderCancelledAsync(orderEvent, args.CancellationToken);
                    break;
                case "Returned":
                    await HandleOrderReturnedAsync(orderEvent, args.CancellationToken);
                    break;
                case "Modified":
                    await HandleOrderModifiedAsync(orderEvent, args.CancellationToken);
                    break;
                default:
                    _logger.LogWarning("Unknown order status: {Status}", orderEvent.OrderStatus);
                    break;
            }

            // Save to CSV history
            await _eventHistory.SaveEventAsync(
                orderEvent,
                eventType: $"OrderStateChanged:{orderEvent.OrderStatus}",
                source: TopicNames.Orders,
                orderId: orderEvent.OrderId.ToString(),
                status: "Processed"
            );
            _logger.LogInformation("Event saved to CSV history");

            await args.CompleteMessageAsync(args.Message);
            _logger.LogInformation("Message completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order event: {Message}", ex.Message);
            await args.AbandonMessageAsync(args.Message);
        }
    }

    private async Task HandleOrderPlacedAsync(OrderStateChangedEventDto orderEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling Order Placed for OrderId: {OrderId}, PremiumSubscription: {PremiumSubscription}", 
            orderEvent.OrderId, orderEvent.PremiumSubscription);

        var shipmentId = Guid.NewGuid();
        var trackingNumber = $"TRACK-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

        // Calculate shipping cost using the operation (explicit state transition)
        var orderTotal = orderEvent.Total > 0 ? orderEvent.Total : orderEvent.TotalPrice; // Fallback for backwards compatibility
        
        // Use CalculateShippingCostOperation for shipping cost calculation
        var shippingCost = CalculateShippingCostOperation.CalculateShippingCost(orderTotal, orderEvent.PremiumSubscription);
        var shippingDescription = CalculateShippingCostOperation.GetShippingCostDescription(orderTotal, orderEvent.PremiumSubscription);
        
        _logger.LogInformation("Shipping calculation: {Description}", shippingDescription);

        var totalWithShipping = orderTotal + shippingCost;
        _logger.LogInformation("Total with shipping: {TotalWithShipping} RON", totalWithShipping);

        // Premium customers get Priority status, regular customers get Scheduled
        var shipmentStatus = orderEvent.PremiumSubscription ? "Priority" : "Scheduled";

        var shipmentEntity = new ShipmentEntity
        {
            ShipmentId = shipmentId,
            OrderId = orderEvent.OrderId,
            UserId = orderEvent.UserId,
            TotalPrice = orderTotal,
            ShippingCost = shippingCost,
            TotalWithShipping = totalWithShipping,
            TrackingNumber = trackingNumber,
            Status = shipmentStatus,
            CreatedAt = DateTime.UtcNow
        };

        var lineEntities = orderEvent.Lines.Select(l => new ShipmentLineEntity
        {
            ShipmentLineId = Guid.NewGuid(),
            ShipmentId = shipmentId,
            Name = l.Name,
            Description = l.Description,
            Category = l.Category,
            Quantity = l.Quantity,
            UnitPrice = l.UnitPrice,
            LineTotal = l.LineTotal
        }).ToList();

        await _repository.SaveShipmentAsync(shipmentEntity, lineEntities, cancellationToken);

        _logger.LogInformation("Shipment created: ShipmentId={ShipmentId}, TrackingNumber={TrackingNumber}, Status={Status}, ShippingCost={ShippingCost}", 
            shipmentId, trackingNumber, shipmentStatus, shippingCost);

        // Publish ShipmentStateChangedEvent with shipping cost info
        var shipmentEvent = new ShipmentStateChangedEvent
        {
            ShipmentState = shipmentStatus,
            ShipmentId = shipmentId,
            OrderId = orderEvent.OrderId,
            UserId = orderEvent.UserId,
            PremiumSubscription = orderEvent.PremiumSubscription,
            PaymentMethod = orderEvent.PaymentMethod,
            TrackingNumber = trackingNumber,
            Subtotal = orderEvent.Subtotal > 0 ? orderEvent.Subtotal : orderTotal,
            DiscountAmount = orderEvent.DiscountAmount,
            TotalAfterDiscount = orderTotal,
            ShippingCost = shippingCost,
            TotalWithShipping = totalWithShipping,
            Lines = orderEvent.Lines.Select(l => new ShipmentLineEventDto
            {
                Name = l.Name,
                Description = l.Description,
                Category = l.Category,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                LineTotal = l.LineTotal
            }).ToList()
        };

        var eventJson = JsonSerializer.Serialize(shipmentEvent, JsonSerializerOptionsProvider.Default);
        var message = new ServiceBusMessage(eventJson)
        {
            ContentType = "application/json",
            Subject = "ShipmentStateChanged",
            MessageId = shipmentEvent.EventId.ToString()
        };

        await _shipmentSender.SendMessageAsync(message, cancellationToken);
        _logger.LogInformation("Published ShipmentStateChangedEvent(Scheduled) to topic '{Topic}'", TopicNames.Shipments);
    }

    private async Task HandleOrderCancelledAsync(OrderStateChangedEventDto orderEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling Order Cancelled for OrderId: {OrderId}, Reason: {Reason}", orderEvent.OrderId, orderEvent.Reason);

        // Update shipment status to Cancelled
        await _repository.UpdateStatusByOrderIdAsync(orderEvent.OrderId, "Cancelled", cancellationToken);

        _logger.LogInformation("Shipment cancelled for OrderId: {OrderId}", orderEvent.OrderId);

        // Publish ShipmentStateChangedEvent(Cancelled)
        var shipmentEvent = new ShipmentStateChangedEvent
        {
            ShipmentState = "Cancelled",
            OrderId = orderEvent.OrderId,
            UserId = orderEvent.UserId,
            Reason = orderEvent.Reason
        };

        var eventJson = JsonSerializer.Serialize(shipmentEvent, JsonSerializerOptionsProvider.Default);
        var message = new ServiceBusMessage(eventJson)
        {
            ContentType = "application/json",
            Subject = "ShipmentStateChanged",
            MessageId = shipmentEvent.EventId.ToString()
        };

        await _shipmentSender.SendMessageAsync(message, cancellationToken);
        _logger.LogInformation("Published ShipmentStateChangedEvent(Cancelled)");
    }

    private async Task HandleOrderReturnedAsync(OrderStateChangedEventDto orderEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling Order Returned for OrderId: {OrderId}", orderEvent.OrderId);

        await _repository.UpdateStatusByOrderIdAsync(orderEvent.OrderId, "Returned", cancellationToken);

        var shipmentEvent = new ShipmentStateChangedEvent
        {
            ShipmentState = "Returned",
            OrderId = orderEvent.OrderId,
            UserId = orderEvent.UserId,
            Reason = orderEvent.Reason
        };

        var eventJson = JsonSerializer.Serialize(shipmentEvent, JsonSerializerOptionsProvider.Default);
        var message = new ServiceBusMessage(eventJson)
        {
            ContentType = "application/json",
            Subject = "ShipmentStateChanged",
            MessageId = shipmentEvent.EventId.ToString()
        };

        await _shipmentSender.SendMessageAsync(message, cancellationToken);
        _logger.LogInformation("Published ShipmentStateChangedEvent(Returned)");
    }

    private Task HandleOrderModifiedAsync(OrderStateChangedEventDto orderEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling Order Modified for OrderId: {OrderId}", orderEvent.OrderId);
        // For now, just log - could update shipment details if needed
        return Task.CompletedTask;
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
