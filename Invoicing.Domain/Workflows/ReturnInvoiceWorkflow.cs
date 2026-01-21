using Invoicing.Events;
using Invoicing.Operations;
using SharedKernel.Invoicing;
using Microsoft.Extensions.Logging;

namespace Invoicing.Workflows;

/// <summary>
/// Workflow for handling returned orders - creates credit note / storno
/// Similar to ReturnOrderWorkflow and ReturnShipmentWorkflow
/// </summary>
public class ReturnInvoiceWorkflow
{
    private readonly IInvoiceRepository _repository;
    private readonly ILogger<ReturnInvoiceWorkflow> _logger;

    public ReturnInvoiceWorkflow(
        IInvoiceRepository repository,
        ILogger<ReturnInvoiceWorkflow> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Creates a credit note / storno for the returned order
    /// </summary>
    public async Task<ReturnInvoiceResult> ExecuteAsync(
        Guid orderId,
        string? reason,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting invoice return (credit note) for Order: {OrderId}, Reason: {Reason}",
                orderId, reason);

            // Update invoice status to Storno/CreditNote
            await _repository.UpdateStatusByOrderIdAsync(orderId, "CreditNote", cancellationToken);

            _logger.LogInformation("Credit note created for Order: {OrderId}", orderId);

            return new ReturnInvoiceResult(
                Success: true,
                OrderId: orderId,
                NewStatus: "CreditNote",
                ErrorMessage: null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating credit note for Order: {OrderId}", orderId);
            
            return new ReturnInvoiceResult(
                Success: false,
                OrderId: orderId,
                NewStatus: "Failed",
                ErrorMessage: ex.Message);
        }
    }
}

/// <summary>
/// Result of invoice return workflow
/// </summary>
public record ReturnInvoiceResult(
    bool Success,
    Guid OrderId,
    string NewStatus,
    string? ErrorMessage);

