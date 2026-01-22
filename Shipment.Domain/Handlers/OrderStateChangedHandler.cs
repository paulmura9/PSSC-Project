using Microsoft.Extensions.Logging;
using SharedKernel;
using SharedKernel.Messaging;
using Shipment.Domain.Events;
using Shipment.Domain.Workflows;

namespace Shipment.Domain.Handlers;

/// <summary>
/// Handler for OrderStateChangedEvent - creates shipments from orders
/// </summary>
public class OrderStateChangedHandler : AbstractEventHandler<OrderStateChangedEvent>
{
    private readonly CreateShipmentWorkflow _workflow;
    private readonly IEventBus _eventBus;
    private readonly IEventHistoryService _eventHistory;
    private readonly ILogger<OrderStateChangedHandler> _logger;
    
    public override string[] EventTypes => new[] { "OrderStateChanged" }; //ce tipuri de evenimete proceseaza

    public OrderStateChangedHandler(
        CreateShipmentWorkflow workflow,
        IEventBus eventBus,
        IEventHistoryService eventHistory,
        ILogger<OrderStateChangedHandler> logger)
    {
        _workflow = workflow;
        _eventBus = eventBus;
        _eventHistory = eventHistory;
        _logger = logger;
    }

    protected override async Task<EventProcessingResult> OnHandleAsync(OrderStateChangedEvent orderEvent, CancellationToken cancellationToken)
    {
        //orderEvent e deserilizat deja, adica obiect
        _logger.LogInformation("Order Event Parsed Successfully:");
        _logger.LogInformation("  - Order ID: {OrderId}", orderEvent.OrderId);
        _logger.LogInformation("  - User ID: {UserId}", orderEvent.UserId);
        _logger.LogInformation("  - Total Price: {TotalPrice:C}", orderEvent.TotalPrice);
        _logger.LogInformation("  - Lines Count: {LinesCount}", orderEvent.Lines.Count);

        // Map to shipment command
        var shipmentLines = orderEvent.Lines
            .Select(l => new ShipmentLineInput(
                l.Name,
                l.Description ?? string.Empty,
                l.Category ?? string.Empty,
                l.Quantity,
                l.UnitPrice,
                l.LineTotal))
            .ToList()
            .AsReadOnly();

        //comanda pt workflow
        var command = new CreateShipmentCommand(
            orderEvent.OrderId,
            orderEvent.UserId,
            orderEvent.PremiumSubscription,
            orderEvent.TotalPrice,
            orderEvent.Subtotal,
            orderEvent.DiscountAmount,
            orderEvent.PaymentMethod,
            shipmentLines,
            orderEvent.OccurredAt);

        _logger.LogInformation("Processing shipment for Order: {OrderId}...", orderEvent.OrderId);

        // Execute workflow - returns IShipmentWorkflowResult
        var result = await _workflow.ExecuteAsync(command, cancellationToken);

        // Handle result based on event type
        return result switch
        {
            ShipmentCreatedSuccessEvent success => await HandleSuccessAsync(success, orderEvent, cancellationToken),
            ShipmentCreatedFailedEvent failed => await HandleFailureAsync(failed, orderEvent, cancellationToken),
            _ => EventProcessingResult.Failed("Unknown event type returned from workflow")
        };
    }

    private async Task<EventProcessingResult> HandleSuccessAsync(
        ShipmentCreatedSuccessEvent success,
        OrderStateChangedEvent orderEvent, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("========================================");
        _logger.LogInformation("SHIPMENT CREATED SUCCESSFULLY");
        _logger.LogInformation("  - Shipment ID: {ShipmentId}", success.ShipmentId);
        _logger.LogInformation("  - Tracking Number: {TrackingNumber}", success.TrackingNumber);
        _logger.LogInformation("  - Order ID: {OrderId}", success.OrderId);
        _logger.LogInformation("========================================");

        // Publish is now done in workflow via PublishShipmentOperation
        
        // Save event to CSV history
        await _eventHistory.SaveEventAsync(
            orderEvent,
            eventType: "OrderStateChanged",
            source: TopicNames.Orders,
            orderId: orderEvent.OrderId.ToString(),
            status: "Processed"
        );
        _logger.LogInformation("Event saved to CSV history");

        return EventProcessingResult.Succeeded();
    }

    private async Task<EventProcessingResult> HandleFailureAsync(
        ShipmentCreatedFailedEvent failed,
        OrderStateChangedEvent orderEvent, 
        CancellationToken cancellationToken)
    {
        var errorMessage = string.Join(", ", failed.Reasons);
        
        _logger.LogWarning("========================================");
        _logger.LogWarning("SHIPMENT PROCESSING FAILED");
        _logger.LogWarning("  - Order ID: {OrderId}", orderEvent.OrderId);
        _logger.LogWarning("  - Error: {Error}", errorMessage);
        _logger.LogWarning("========================================");
        
        // Save failed event to CSV
        await _eventHistory.SaveEventAsync(
            orderEvent,
            eventType: "OrderStateChanged",
            source: TopicNames.Orders,
            orderId: orderEvent.OrderId.ToString(),
            status: $"Failed: {errorMessage}"
        );
        
        return EventProcessingResult.Failed(errorMessage);
    }
}
