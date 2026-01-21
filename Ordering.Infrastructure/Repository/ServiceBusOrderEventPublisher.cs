using Microsoft.Extensions.Logging;
using Ordering.Domain.Events;
using Ordering.Domain.Operations;
using SharedKernel;
using SharedKernel.ServiceBus;

namespace Ordering.Infrastructure.Repository;

/// <summary>
/// Service Bus implementation of IOrderEventPublisher
/// Publishes single OrderStateChangedEvent with different Status values
/// Also saves to CSV history for local tracking
/// </summary>
public class ServiceBusOrderEventPublisher : IOrderEventPublisher
{
    private readonly IEventBus _eventBus;
    private readonly IEventHistoryService _eventHistory;
    private readonly ILogger<ServiceBusOrderEventPublisher> _logger;

    public ServiceBusOrderEventPublisher(
        IEventBus eventBus, 
        IEventHistoryService eventHistory,
        ILogger<ServiceBusOrderEventPublisher> logger)
    {
        _eventBus = eventBus;
        _eventHistory = eventHistory;
        _logger = logger;
    }

    public async Task PublishOrderStateChangedAsync(OrderStateChangedEvent eventDto, CancellationToken cancellationToken)
    {
        // Publish to Service Bus
        await _eventBus.PublishAsync(TopicNames.Orders, eventDto, cancellationToken);
        _logger.LogInformation("Published OrderStateChanged:{Status} to topic '{Topic}' for OrderId: {OrderId}", 
            eventDto.OrderStatus, TopicNames.Orders, eventDto.OrderId);
        
        // Save to CSV history
        await _eventHistory.SaveEventAsync(
            eventDto,
            eventType: $"OrderStateChanged:{eventDto.OrderStatus}",
            source: "Ordering.Api",
            orderId: eventDto.OrderId.ToString(),
            status: "Published"
        );
        _logger.LogInformation("Event saved to CSV history for OrderId: {OrderId}", eventDto.OrderId);
    }
}
