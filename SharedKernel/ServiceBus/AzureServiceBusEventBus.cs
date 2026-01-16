using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SharedKernel.ServiceBus;

/// <summary>
/// Azure Service Bus implementation of IEventBus
/// </summary>
public class AzureServiceBusEventBus : IEventBus, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly Dictionary<string, ServiceBusSender> _senders = new();
    private readonly ILogger<AzureServiceBusEventBus> _logger;
    private readonly object _lock = new();

    public AzureServiceBusEventBus(IConfiguration configuration, ILogger<AzureServiceBusEventBus> logger)
    {
        _logger = logger;
        
        var connectionString = configuration["ServiceBus:ConnectionString"]
            ?? throw new InvalidOperationException("ServiceBus:ConnectionString is not configured");

        _client = new ServiceBusClient(connectionString);
        _logger.LogInformation("Azure Service Bus client initialized");
    }

    public async Task PublishAsync<TEvent>(string topicName, TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        var sender = GetOrCreateSender(topicName);

        var eventTypeName = @event.GetType().Name;
        var messageBody = JsonSerializer.Serialize(@event, JsonSerializerOptionsProvider.Default);

        var message = new ServiceBusMessage(messageBody)
        {
            Subject = eventTypeName,
            ContentType = "application/json",
            MessageId = @event.EventId.ToString(),
            CorrelationId = @event.EventId.ToString()
        };

        message.ApplicationProperties["EventType"] = eventTypeName;

        await sender.SendMessageAsync(message, cancellationToken);
        
        _logger.LogInformation("Published {EventType} to topic {TopicName} with MessageId {MessageId}", 
            eventTypeName, topicName, message.MessageId);
    }

    private ServiceBusSender GetOrCreateSender(string topicName)
    {
        lock (_lock)
        {
            if (!_senders.TryGetValue(topicName, out var sender))
            {
                sender = _client.CreateSender(topicName);
                _senders[topicName] = sender;
            }
            return sender;
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var sender in _senders.Values)
        {
            await sender.DisposeAsync();
        }
        _senders.Clear();
        await _client.DisposeAsync();
    }
}

