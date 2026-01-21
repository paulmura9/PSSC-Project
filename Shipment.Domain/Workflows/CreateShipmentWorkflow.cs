using Microsoft.Extensions.Logging;
using Shipment.Domain.Events;
using Shipment.Domain.Models;
using Shipment.Domain.Operations;
using SharedKernel.Shipment;
using static Shipment.Domain.Models.Shipment;

namespace Shipment.Domain.Workflows;

/// <summary>
/// Workflow for creating shipments from order events
/// State transitions: Created -> ShippingCostCalculated -> Scheduled -> Persisted
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
    private readonly IShipmentRepository? _repository;

    public CreateShipmentWorkflow(ILogger<CreateShipmentWorkflow> logger, IShipmentRepository? repository = null)
    {
        _logger = logger;
        _repository = repository;
    }

    /// <summary>
    /// Executes the shipment creation workflow (Lab pattern)
    /// Returns IShipmentSentEvent using ToEvent()
    /// </summary>
    public async Task<IShipmentSentEvent> ExecuteAsync(
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

            // Step 2: Execute pure business logic through operations (SYNC - no I/O)
            IShipment shipment = ExecuteBusinessLogic(createdShipment, command.IsPremium);

            // Step 3: Save to database if we have a scheduled shipment
            if (shipment is ScheduledShipment scheduled && _repository != null)
            {
                var persisted = new PersistedShipment(
                    shipmentId: scheduled.ShipmentId,
                    orderId: scheduled.OrderId,
                    userId: scheduled.UserId,
                    totalPrice: scheduled.TotalPrice,
                    shippingCost: scheduled.ShippingCost,
                    totalWithShipping: scheduled.TotalWithShipping,
                    lines: scheduled.Lines,
                    trackingNumber: scheduled.TrackingNumber,
                    orderPlacedAt: scheduled.OrderPlacedAt,
                    persistedAt: DateTime.UtcNow);

                // Map to save data DTO
                var saveData = new ShipmentSaveData
                {
                    ShipmentId = persisted.ShipmentId,
                    OrderId = persisted.OrderId,
                    UserId = persisted.UserId,
                    TrackingNumber = persisted.TrackingNumber.Value,
                    TotalPrice = persisted.TotalPrice.Value,
                    ShippingCost = persisted.ShippingCost.Value,
                    TotalWithShipping = persisted.TotalWithShipping.Value,
                    Status = "Scheduled",
                    Lines = persisted.Lines.Select(l => new ShipmentLineSaveData
                    {
                        ShipmentLineId = Guid.NewGuid(),
                        Name = l.Name.Value,
                        Description = l.Description,
                        Category = l.Category,
                        Quantity = l.Quantity.Value,
                        UnitPrice = l.UnitPrice.Value,
                        LineTotal = l.LineTotal.Value
                    }).ToList()
                };

                await _repository.SaveShipmentAsync(saveData, cancellationToken);
                _logger.LogInformation("Shipment persisted: {ShipmentId}", persisted.ShipmentId);

                // Return event using ToEvent() (Lab pattern)
                return persisted.ToEvent();
            }

            // Return event using ToEvent() for non-persisted states
            return shipment.ToEvent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating shipment for Order: {OrderId}", command.OrderId);
            return new ShipmentSendFailedEvent
            {
                OrderId = command.OrderId,
                Reasons = new[] { ex.Message }
            };
        }
    }

    /// <summary>
    /// Executes pure business logic through operations (SYNC - no I/O)
    /// Created -> ShippingCostCalculated -> Scheduled
    /// </summary>
    private static IShipment ExecuteBusinessLogic(CreatedShipment createdShipment, bool isPremium)
    {
        IShipment shipment = new CalculateShippingCostOperation().Transform(createdShipment, PremiumStatus.Create(isPremium));
        shipment = new ScheduleShipmentOperation().Transform(shipment);
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

