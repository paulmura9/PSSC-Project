using Microsoft.Extensions.Logging;
using Shipment.Domain.Events;
using Shipment.Domain.Models;
using Shipment.Domain.Operations;
using static Shipment.Domain.Models.Shipment;

namespace Shipment.Domain.Workflows;

/// <summary>
/// Workflow for creating shipments from order events
/// State transitions: Created -> ShippingCostCalculated -> Scheduled -> Persisted -> Published
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
    private readonly PersistShipmentOperation _persistOperation;
    private readonly PublishShipmentOperation _publishOperation;

    public CreateShipmentWorkflow(
        ILogger<CreateShipmentWorkflow> logger, 
        PersistShipmentOperation persistOperation,
        PublishShipmentOperation publishOperation)
    {
        _logger = logger;
        _persistOperation = persistOperation;
        _publishOperation = publishOperation;
    }

    /// <summary>
    /// Executes the shipment creation workflow (Lab pattern)
    /// Returns IShipmentSentEvent using ToEvent()
    /// </summary>
    public async Task<IShipmentWorkflowResult> ExecuteAsync(
        CreateShipmentCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting shipment creation for Order: {OrderId}", command.OrderId);

            // Step 1: Create initial shipment state (VO)
            var createdShipment = new CreatedShipment(
                orderId: command.OrderId,
                userId: command.UserId,
                premiumSubscription: command.IsPremium,
                totalPrice: new Money(command.OrderTotal),
                lines: command.Lines.Select(l => new ShipmentLine(
                    new ProductName(l.Name),
                    new ProductDescription(l.Description),
                    new ProductCategory(l.Category),
                    new Quantity(l.Quantity),
                    new Money(l.UnitPrice),
                    new Money(l.LineTotal)
                )).ToList().AsReadOnly(),
                orderPlacedAt: command.OrderPlacedAt);

            _logger.LogInformation("Created shipment state for Order: {OrderId}", command.OrderId);

            // Step 2: Execute pure business logic through operations 
            IShipment shipment = ExecuteBusinessLogic(createdShipment);

            // Step 3: Persist to database (Scheduled/Dispatched -> PersistedShipment)
            var persisted = await _persistOperation.ExecuteAsync(shipment, cancellationToken) as PersistedShipment;
            if (persisted == null)
            {
                return new ShipmentCreatedFailedEvent { Reasons = new[] { "Failed to persist shipment" } };
            }
            _logger.LogInformation("Shipment persisted: {ShipmentId}", persisted.ShipmentId);

            // Step 4: Publish to Service Bus (PersistedShipment -> PublishedShipment)
            var published = await _publishOperation.ExecuteAsync(
                persisted, 
                command.IsPremium, 
                command.PaymentMethod,
                command.Subtotal,
                command.DiscountAmount,
                cancellationToken) as PublishedShipment;
            
            if (published == null)
            {
                return new ShipmentCreatedFailedEvent { Reasons = new[] { "Failed to publish shipment" } };
            }
            _logger.LogInformation("Shipment published: {ShipmentId}", published.ShipmentId);

            // Return event using ToEvent() (Lab pattern)
            return published.ToEvent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating shipment for Order: {OrderId}", command.OrderId);
            return new ShipmentCreatedFailedEvent
            {
                OrderId = command.OrderId,
                Reasons = new[] { ex.Message }
            };
        }
    }

    /// <summary>
    /// Executes pure business logic through operations (SYNC - no I/O)
    /// Created -> ShippingCostCalculated -> Scheduled -> (Dispatched for Premium)
    /// </summary>
    private static IShipment ExecuteBusinessLogic(CreatedShipment createdShipment)
    {
        IShipment shipment = new CalculateShippingCostOperation().Transform(createdShipment);
        shipment = new ScheduleShipmentOperation().Transform(shipment);
        
        // Premium customers get dispatched immediately (priority shipping)
        if (createdShipment.PremiumSubscription)
        {
            shipment = new DispatchShipmentOperation().Transform(shipment);
        }
        
        return shipment;
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
    string PaymentMethod,
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

