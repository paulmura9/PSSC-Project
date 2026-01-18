using Microsoft.Extensions.Logging;
using Shipment.Domain.Models;
using Shipment.Domain.Operations;
using static Shipment.Domain.Models.Shipment;

namespace Shipment.Domain.Workflows;

/// <summary>
/// Workflow for creating shipments from order events
/// State transitions: Created -> ShippingCostCalculated -> Scheduled -> Dispatched -> Persisted
/// 
/// Shipping cost rules (in RON):
/// - Premium customers: FREE (0 RON)
/// - Regular customers based on order total:
///   - 0-3000 RON: 30 RON
///   - 3001-6000 RON: 50 RON
///   - 6001-10000 RON: 75 RON
///   - >= 10001 RON: 100 RON
/// </summary>
public class CreateShipmentWorkflow
{
    private readonly ILogger<CreateShipmentWorkflow> _logger;

    public CreateShipmentWorkflow(ILogger<CreateShipmentWorkflow> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Executes the shipment creation workflow
    /// </summary>
    public Task<CreateShipmentResult> ExecuteAsync(
        CreateShipmentCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting shipment creation for Order: {OrderId}", command.OrderId);

            // Step 1: Create initial shipment state
            var createdShipment = new CreatedShipment(
                orderId: command.OrderId,
                userId: command.UserId,
                totalPrice: new Money(command.OrderTotal),
                lines: command.Lines.Select(l => new ShipmentLine(
                    new ProductName(l.Name),
                    l.Description,
                    l.Category,
                    new Quantity(l.Quantity),
                    new Money(l.UnitPrice),
                    new Money(l.LineTotal)
                )).ToList().AsReadOnly(),
                orderPlacedAt: command.OrderPlacedAt);

            _logger.LogInformation("Created shipment state for Order: {OrderId}", command.OrderId);

            // Step 2: Calculate shipping cost
            var shippingCost = CalculateShippingCostOperation.CalculateShippingCost(
                command.OrderTotal, 
                command.IsPremium);
            
            var shippingDescription = CalculateShippingCostOperation.GetShippingCostDescription(
                command.OrderTotal, 
                command.IsPremium);
            
            _logger.LogInformation("Shipping calculation: {Description}", shippingDescription);

            var totalWithShipping = command.OrderTotal + shippingCost;

            var shippingCostCalculated = new ShippingCostCalculatedShipment(
                orderId: command.OrderId,
                userId: command.UserId,
                premiumSubscription: command.IsPremium,
                orderTotal: new Money(command.OrderTotal),
                shippingCost: new Money(shippingCost),
                totalWithShipping: new Money(totalWithShipping),
                lines: createdShipment.Lines,
                orderPlacedAt: command.OrderPlacedAt,
                calculatedAt: DateTime.UtcNow);

            _logger.LogInformation("Shipping cost calculated: {ShippingCost} RON, Total with shipping: {Total} RON",
                shippingCost, totalWithShipping);

            // Step 3: Schedule shipment (assign tracking number)
            var shipmentId = Guid.NewGuid();
            var trackingNumber = TrackingNumber.Generate();
            
            // Premium customers get Priority status
            var status = command.IsPremium ? "Priority" : "Scheduled";

            var scheduledShipment = new ScheduledShipment(
                shipmentId: shipmentId,
                orderId: command.OrderId,
                userId: command.UserId,
                totalPrice: new Money(totalWithShipping),
                lines: createdShipment.Lines,
                trackingNumber: trackingNumber,
                orderPlacedAt: command.OrderPlacedAt,
                scheduledAt: DateTime.UtcNow);

            _logger.LogInformation("Shipment scheduled: ShipmentId={ShipmentId}, TrackingNumber={TrackingNumber}, Status={Status}",
                shipmentId, trackingNumber.Value, status);

            // Return successful result
            var result = new CreateShipmentResult(
                Success: true,
                ShipmentId: shipmentId,
                TrackingNumber: trackingNumber.Value,
                ShippingCost: shippingCost,
                TotalWithShipping: totalWithShipping,
                Status: status,
                ErrorMessage: null);

            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating shipment for Order: {OrderId}", command.OrderId);
            
            return Task.FromResult(new CreateShipmentResult(
                Success: false,
                ShipmentId: Guid.Empty,
                TrackingNumber: string.Empty,
                ShippingCost: 0,
                TotalWithShipping: 0,
                Status: "Failed",
                ErrorMessage: ex.Message));
        }
    }
}

/// <summary>
/// Command to create a shipment from an order event
/// </summary>
public record CreateShipmentCommand(
    Guid OrderId,
    Guid UserId,
    bool IsPremium,
    decimal OrderTotal,
    decimal Subtotal,
    decimal DiscountAmount,
    IReadOnlyCollection<ShipmentLineInput> Lines,
    DateTime OrderPlacedAt);

/// <summary>
/// Input DTO for shipment line
/// </summary>
public record ShipmentLineInput(
    string Name,
    string Description,
    string Category,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);

/// <summary>
/// Result of shipment creation workflow
/// </summary>
public record CreateShipmentResult(
    bool Success,
    Guid ShipmentId,
    string TrackingNumber,
    decimal ShippingCost,
    decimal TotalWithShipping,
    string Status,
    string? ErrorMessage);

