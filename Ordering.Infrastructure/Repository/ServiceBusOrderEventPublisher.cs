using Ordering.Domain.Events;
using Ordering.Domain.Operations;
using SharedKernel;

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

    public ServiceBusOrderEventPublisher(IEventBus eventBus, IEventHistoryService eventHistory)
    {
        _eventBus = eventBus;
        _eventHistory = eventHistory;
    }

    public async Task PublishOrderStateChangedAsync(OrderStateChangedEvent eventDto, CancellationToken cancellationToken)
    {
        // Publish to Service Bus
        await _eventBus.PublishAsync(TopicNames.Orders, eventDto, cancellationToken);
        
        // Save to CSV history
        await _eventHistory.SaveEventAsync(
            eventDto,
            eventType: $"OrderStateChanged:{eventDto.OrderStatus}",
            source: "Ordering.Api",
            orderId: eventDto.OrderId.ToString(),
            status: "Published"
        );
    }
}
