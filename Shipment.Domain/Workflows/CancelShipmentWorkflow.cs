using Microsoft.Extensions.Logging;
using Shipment.Domain.Operations;

namespace Shipment.Domain.Workflows;

/// <summary>
/// Workflow for cancelling shipments when order is cancelled
/// Transitions: Any state -> Cancelled (if not already Dispatched)
/// </summary>
public class CancelShipmentWorkflow
{
    private readonly IShipmentRepository _repository;
    private readonly ILogger<CancelShipmentWorkflow> _logger;

    public CancelShipmentWorkflow(
        IShipmentRepository repository,
        ILogger<CancelShipmentWorkflow> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Cancels a shipment for the given order
    /// </summary>
    public async Task<CancelShipmentResult> ExecuteAsync(
        Guid orderId,
        string? reason,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting shipment cancellation for Order: {OrderId}, Reason: {Reason}",
                orderId, reason);

            // Update shipment status to Cancelled
            await _repository.UpdateStatusByOrderIdAsync(orderId, "Cancelled", cancellationToken);

            _logger.LogInformation("Shipment cancelled for Order: {OrderId}", orderId);

            return new CancelShipmentResult(
                Success: true,
                OrderId: orderId,
                NewStatus: "Cancelled",
                ErrorMessage: null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling shipment for Order: {OrderId}", orderId);
            
            return new CancelShipmentResult(
                Success: false,
                OrderId: orderId,
                NewStatus: "Failed",
                ErrorMessage: ex.Message);
        }
    }
}

/// <summary>
/// Result of shipment cancellation workflow
/// </summary>
public record CancelShipmentResult(
    bool Success,
    Guid OrderId,
    string NewStatus,
    string? ErrorMessage);

