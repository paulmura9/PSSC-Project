using Microsoft.Extensions.Logging;
using SharedKernel;
using SharedKernel.Messaging;
using Invoicing.Models;
using Invoicing.Workflows;
using Invoicing.Events;

namespace Invoicing.Handlers;

/// <summary>
/// Handler for ShipmentStateChangedEvent - creates invoices from shipments
/// Structure mirrors OrderStateChangedHandler in Shipment
/// </summary>
public class ShipmentStateChangedHandler : AbstractEventHandler<ShipmentStateChangedEvent>
{
    private readonly CreateInvoiceWorkflow _workflow;
    private readonly IEventHistoryService _eventHistory;
    private readonly ILogger<ShipmentStateChangedHandler> _logger;

    public override string[] EventTypes => new[] { "ShipmentStateChanged" };

    public ShipmentStateChangedHandler(
        CreateInvoiceWorkflow workflow,
        IEventHistoryService eventHistory,
        ILogger<ShipmentStateChangedHandler> logger)
    {
        _workflow = workflow;
        _eventHistory = eventHistory;
        _logger = logger;
    }

    protected override async Task<EventProcessingResult> OnHandleAsync(ShipmentStateChangedEvent shipmentEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Shipment Event Received:");
        _logger.LogInformation("  Shipment ID: {ShipmentId}", shipmentEvent.ShipmentId);
        _logger.LogInformation("  Order ID: {OrderId}", shipmentEvent.OrderId);
        _logger.LogInformation("  State: {State}", shipmentEvent.ShipmentState);
        _logger.LogInformation("  Tracking: {TrackingNumber}", shipmentEvent.TrackingNumber);
        _logger.LogInformation("  PaymentMethod: {PaymentMethod}", shipmentEvent.PaymentMethod);
        _logger.LogInformation("  Lines Count: {LinesCount}", shipmentEvent.Lines.Count);

        // Map to invoice command
        var invoiceLines = shipmentEvent.Lines
            .Select(l => new InvoiceLineInput(
                l.Name,
                l.Description ?? string.Empty,
                l.Category ?? string.Empty,
                l.Quantity,
                l.UnitPrice))
            .ToList()
            .AsReadOnly();

        // Determine payment status based on payment method
        var paymentStatus = shipmentEvent.PaymentMethod.Equals("CardOnline", StringComparison.OrdinalIgnoreCase)
            ? "Authorized"
            : "Pending";

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
            invoiceLines,
            shipmentEvent.OccurredAt,
            paymentStatus);

        _logger.LogInformation("Processing invoice for Shipment: {ShipmentId}, PaymentStatus: {PaymentStatus}", 
            shipmentEvent.ShipmentId, paymentStatus);

        // Execute workflow - returns IInvoiceWorkflowResult
        var result = await _workflow.ExecuteAsync(command, cancellationToken);

        // Handle result based on event type
        return result switch
        {
            InvoiceCreatedSuccessEvent success => await HandleSuccessAsync(success, shipmentEvent, cancellationToken),
            InvoiceCreatedFailedEvent failed => await HandleFailureAsync(failed, shipmentEvent, cancellationToken),
            _ => EventProcessingResult.Failed("Unknown event type returned from workflow")
        };
    }

    private async Task<EventProcessingResult> HandleSuccessAsync(
        InvoiceCreatedSuccessEvent success,
        ShipmentStateChangedEvent shipmentEvent,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("========================================");
        _logger.LogInformation("INVOICE CREATED SUCCESSFULLY");
        _logger.LogInformation("  Invoice ID: {InvoiceId}", success.InvoiceId);
        _logger.LogInformation("  Invoice Number: {InvoiceNumber}", success.InvoiceNumber);
        _logger.LogInformation("  Order ID: {OrderId}", success.OrderId);
        _logger.LogInformation("  Shipment ID: {ShipmentId}", success.ShipmentId);
        _logger.LogInformation("========================================");

        // Save event to CSV history (publish is done in workflow)
        await _eventHistory.SaveEventAsync(
            shipmentEvent,
            eventType: $"ShipmentStateChanged:{shipmentEvent.ShipmentState}",
            source: TopicNames.Shipments,
            orderId: shipmentEvent.OrderId.ToString(),
            status: "Processed"
        );
        _logger.LogInformation("Event saved to CSV history");

        return EventProcessingResult.Succeeded();
    }

    private async Task<EventProcessingResult> HandleFailureAsync(
        InvoiceCreatedFailedEvent failed,
        ShipmentStateChangedEvent shipmentEvent,
        CancellationToken cancellationToken)
    {
        var errorMessage = string.Join(", ", failed.Reasons);

        _logger.LogWarning("========================================");
        _logger.LogWarning("INVOICE CREATION FAILED");
        _logger.LogWarning("  Shipment ID: {ShipmentId}", shipmentEvent.ShipmentId);
        _logger.LogWarning("  Order ID: {OrderId}", shipmentEvent.OrderId);
        _logger.LogWarning("  Error: {Error}", errorMessage);
        _logger.LogWarning("========================================");

        // Save failed event to CSV
        await _eventHistory.SaveEventAsync(
            shipmentEvent,
            eventType: $"ShipmentStateChanged:{shipmentEvent.ShipmentState}",
            source: TopicNames.Shipments,
            orderId: shipmentEvent.OrderId.ToString(),
            status: $"Failed: {errorMessage}"
        );

        return EventProcessingResult.Failed(errorMessage);
    }
}
