using Shipment.Domain.Models;
using Shipment.Domain.Operations;
using Shipment.Infrastructure.Persistence;
using static Shipment.Domain.Events.ShipmentProcessedEvent;
using static Shipment.Domain.Models.Shipment;

namespace Shipment.Domain.Workflows;

/// <summary>
/// Workflow for processing shipments following the DDD pattern
/// </summary>
public class ProcessShipmentWorkflow
{
    private readonly ValidateShipmentOperation _validateOperation;
    private readonly ProcessShipmentOperation _processOperation;
    private readonly PersistShipmentOperation _persistOperation;
    private readonly IShipmentRepository _repository;

    public ProcessShipmentWorkflow(
        ValidateShipmentOperation validateOperation,
        ProcessShipmentOperation processOperation,
        PersistShipmentOperation persistOperation,
        IShipmentRepository repository)
    {
        _validateOperation = validateOperation;
        _processOperation = processOperation;
        _persistOperation = persistOperation;
        _repository = repository;
    }

    public async Task<ShipmentProcessEvent> ExecuteAsync(
        ProcessShipmentCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Create initial unprocessed shipment from command
            IShipment shipment = new UnprocessedShipment(
                command.OrderId,
                command.UserId,
                command.TotalPrice,
                command.Lines,
                command.OrderPlacedAt);

            // Step 1: Validate - transforms UnprocessedShipment to ValidatedShipment/InvalidShipment
            shipment = await _validateOperation.TransformAsync(shipment, cancellationToken);

            if (shipment is InvalidShipment invalid)
            {
                return new ShipmentProcessFailedEvent
                {
                    Reasons = invalid.Reasons
                };
            }

            // Step 2: Process - transforms ValidatedShipment to ProcessedShipment
            shipment = await _processOperation.TransformAsync(shipment, cancellationToken);

            // Step 3: Persist - transforms ProcessedShipment to PersistedShipment
            shipment = await _persistOperation.TransformAsync(shipment, _repository, cancellationToken);

            if (shipment is PersistedShipment persisted)
            {
                return new ShipmentProcessSucceededEvent
                {
                    ShipmentId = persisted.ShipmentId,
                    TrackingNumber = persisted.TrackingNumber
                };
            }

            return new ShipmentProcessFailedEvent
            {
                Reasons = new[] { "Failed to persist shipment" }
            };
        }
        catch (Exception ex)
        {
            return new ShipmentProcessFailedEvent
            {
                Reasons = new[] { ex.Message }
            };
        }
    }
}

