using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharedKernel;
using SharedKernel.ServiceBus;
using Shipment.Domain.Handlers;

namespace Shipment.Domain;

/// <summary>
/// Background service that listens to Order events from Service Bus and processes shipments
/// </summary>
public class OrderEventProcessor : BackgroundService
{
    private readonly ServiceBusProcessor _processor;
    private readonly OrderStateChangedHandler _handler;
    private readonly ILogger<OrderEventProcessor> _logger;

    public OrderEventProcessor(
        ServiceBusClient serviceBusClient,
        OrderStateChangedHandler handler,
        ILogger<OrderEventProcessor> logger)
    {
        _handler = handler;
        _logger = logger;

        // Create processor for the orders topic with subscription
        _processor = serviceBusClient.CreateProcessor(
            topicName: TopicNames.Orders,
            subscriptionName: SubscriptionNames.OrderProcessor,
            new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false,   //procesez unu pe rand
                MaxConcurrentCalls = 1
            });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor.ProcessMessageAsync += ProcessMessageAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;

        _logger.LogInformation("========================================");
        _logger.LogInformation("Shipment Service Started");
        _logger.LogInformation("Listening on Topic: '{Topic}', Subscription: '{Subscription}'", TopicNames.Orders, SubscriptionNames.OrderProcessor);
        _logger.LogInformation("Handler: {HandlerType} for events: {EventTypes}", _handler.GetType().Name, string.Join(", ", _handler.EventTypes));
        _logger.LogInformation("Waiting for order events from Service Bus...");
        _logger.LogInformation("========================================");
        
        //pornesc ascultarea
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
                //marchez mesajul ca procesat cu succes
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
        await _processor.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }
}
