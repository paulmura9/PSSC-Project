using Invoicing.Events;
using Invoicing.Operations;
using Microsoft.Extensions.Logging;

namespace Invoicing.Workflows;

/// <summary>
/// Workflow for cancelling invoices when order/shipment is cancelled
/// Similar to CancelOrderWorkflow and CancelShipmentWorkflow
/// </summary>
public class CancelInvoiceWorkflow
{
    private readonly IInvoiceRepository _repository;
    private readonly ILogger<CancelInvoiceWorkflow> _logger;

    public CancelInvoiceWorkflow(
        IInvoiceRepository repository,
        ILogger<CancelInvoiceWorkflow> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Cancels an invoice for the given order
    /// </summary>
    public async Task<CancelInvoiceResult> ExecuteAsync(
        Guid orderId,
        string? reason,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting invoice cancellation for Order: {OrderId}, Reason: {Reason}",
                orderId, reason);

            // Update invoice status to Cancelled
            await _repository.UpdateStatusByOrderIdAsync(orderId, "Cancelled", cancellationToken);

            _logger.LogInformation("Invoice cancelled for Order: {OrderId}", orderId);

            return new CancelInvoiceResult(
                Success: true,
                OrderId: orderId,
                NewStatus: "Cancelled",
                ErrorMessage: null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling invoice for Order: {OrderId}", orderId);
            
            return new CancelInvoiceResult(
                Success: false,
                OrderId: orderId,
                NewStatus: "Failed",
                ErrorMessage: ex.Message);
        }
    }
}

/// <summary>
/// Result of invoice cancellation workflow
/// </summary>
public record CancelInvoiceResult(
    bool Success,
    Guid OrderId,
    string NewStatus,
    string? ErrorMessage);

