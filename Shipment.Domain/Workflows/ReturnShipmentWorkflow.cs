using Microsoft.Extensions.Logging;
using Shipment.Domain.Operations;
using SharedKernel.Shipment;

namespace Shipment.Domain.Workflows;

/// <summary>
/// Workflow for handling returned orders
/// Creates a return shipment or marks existing shipment as returned
/// </summary>
public class ReturnShipmentWorkflow
{
    private readonly IShipmentRepository _repository;
    private readonly ILogger<ReturnShipmentWorkflow> _logger;

    public ReturnShipmentWorkflow(
        IShipmentRepository repository,
        ILogger<ReturnShipmentWorkflow> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Marks a shipment as returned for the given order
    /// </summary>
    public async Task<ReturnShipmentResult> ExecuteAsync(
        Guid orderId,
        string? reason,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting shipment return for Order: {OrderId}, Reason: {Reason}",
                orderId, reason);

            // Update shipment status to Returned
            await _repository.UpdateStatusByOrderIdAsync(orderId, "Returned", cancellationToken);

            _logger.LogInformation("Shipment marked as returned for Order: {OrderId}", orderId);

            return new ReturnShipmentResult(
                Success: true,
                OrderId: orderId,
                NewStatus: "Returned",
                ErrorMessage: null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing return for Order: {OrderId}", orderId);
            
            return new ReturnShipmentResult(
                Success: false,
                OrderId: orderId,
                NewStatus: "Failed",
                ErrorMessage: ex.Message);
        }
    }
}

/// <summary>
/// Result of shipment return workflow
/// </summary>
public record ReturnShipmentResult(
    bool Success,
    Guid OrderId,
    string NewStatus,
    string? ErrorMessage);

