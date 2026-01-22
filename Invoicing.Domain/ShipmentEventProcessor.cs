using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharedKernel;
using SharedKernel.ServiceBus;
using Invoicing.Handlers;

namespace Invoicing;

/// <summary>
/// Background service that listens to Shipment events from Service Bus and processes invoices
/// Uses AbstractEventHandler pattern from Lab
/// </summary>
public class ShipmentEventProcessor : BackgroundService
{
    private readonly ServiceBusProcessor _processor;
    private readonly ShipmentStateChangedHandler _handler;
    private readonly ILogger<ShipmentEventProcessor> _logger;

    public ShipmentEventProcessor(
        ServiceBusClientFactory clientFactory,
        ShipmentStateChangedHandler handler,
        ILogger<ShipmentEventProcessor> logger)
    {
        _handler = handler;
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
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor.ProcessMessageAsync += ProcessMessageAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;

        _logger.LogInformation("========================================");
        _logger.LogInformation("Invoicing Service Started");
        _logger.LogInformation("Listening on Topic: '{Topic}', Subscription: '{Subscription}'", TopicNames.Shipments, SubscriptionNames.ShipmentProcessor);
        _logger.LogInformation("Handler: {HandlerType} for events: {EventTypes}", _handler.GetType().Name, string.Join(", ", _handler.EventTypes));
        _logger.LogInformation("Waiting for shipment events from Service Bus...");
        _logger.LogInformation("========================================");
        
        await _processor.StartProcessingAsync(stoppingToken);

        // Keep the service running
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }

        _logger.LogInformation("Stopping Shipment Event Processor...");
        await _processor.StopProcessingAsync(stoppingToken);
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        //extrag mesajul/json (CONSUM)
        string messageBody = args.Message.Body.ToString();
        
        _logger.LogInformation("========================================");
        _logger.LogInformation("SERVICE BUS MESSAGE RECEIVED");
        _logger.LogInformation("Message ID: {MessageId}", args.Message.MessageId);
        _logger.LogInformation("========================================");

        try
        {
            // Use AbstractEventHandler to process the event
            var result = await _handler.HandleAsync(messageBody, args.CancellationToken);

            if (result.Success)
            {
                await args.CompleteMessageAsync(args.Message);
                _logger.LogInformation("Message processed successfully");
            }
            else
            {
                _logger.LogWarning("Handler returned failure: {Error}", result.ErrorMessage);
                await args.AbandonMessageAsync(args.Message);
                _logger.LogWarning("Message abandoned - will be retried");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing shipment event: {ErrorMessage}", ex.Message);
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
        await _processor.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }
}

