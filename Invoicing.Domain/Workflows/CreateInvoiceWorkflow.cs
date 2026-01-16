using Invoicing.Models;
using Invoicing.Operations;
using Microsoft.Extensions.Logging;
using static Invoicing.Events.InvoiceProcessedEvent;
using static Invoicing.Models.Invoice;

namespace Invoicing.Workflows;

/// <summary>
/// Workflow for creating invoices following the DDD pattern
/// </summary>
public class CreateInvoiceWorkflow
{
    private readonly ValidateInvoiceOperation _validateOperation;
    private readonly GenerateInvoiceOperation _generateOperation;
    private readonly ILogger<CreateInvoiceWorkflow> _logger;

    public CreateInvoiceWorkflow(
        ValidateInvoiceOperation validateOperation,
        GenerateInvoiceOperation generateOperation,
        ILogger<CreateInvoiceWorkflow> logger)
    {
        _validateOperation = validateOperation;
        _generateOperation = generateOperation;
        _logger = logger;
    }

    public async Task<InvoiceProcessEvent> ExecuteAsync(
        CreateInvoiceCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting invoice creation for Shipment: {ShipmentId}", command.ShipmentId);

            // Create initial unprocessed invoice from command
            var unprocessedInvoice = new UnprocessedInvoice(
                command.ShipmentId,
                command.OrderId,
                command.UserId,
                command.TrackingNumber,
                command.TotalPrice,
                command.Lines,
                command.ShipmentCreatedAt);

            // Step 1: Validate
            var validationResult = await _validateOperation.ExecuteAsync(unprocessedInvoice, cancellationToken);
            _logger.LogInformation("Validation result: {Result}", validationResult.GetType().Name);

            if (validationResult is InvalidInvoice invalid)
            {
                return new InvoiceCreationFailedEvent
                {
                    ShipmentId = command.ShipmentId,
                    Reasons = invalid.Reasons
                };
            }

            // Step 2: Generate invoice
            var generateResult = await _generateOperation.ExecuteAsync((ValidatedInvoice)validationResult, cancellationToken);
            _logger.LogInformation("Generate result: {Result}", generateResult.GetType().Name);

            if (generateResult is GeneratedInvoice generated)
            {
                return new InvoiceCreatedSuccessfullyEvent
                {
                    InvoiceId = generated.InvoiceId,
                    InvoiceNumber = generated.InvoiceNumber,
                    ShipmentId = generated.ShipmentId,
                    OrderId = generated.OrderId,
                    TotalAmount = generated.TotalAmount
                };
            }

            return new InvoiceCreationFailedEvent
            {
                ShipmentId = command.ShipmentId,
                Reasons = new[] { "Failed to generate invoice" }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating invoice for Shipment: {ShipmentId}", command.ShipmentId);
            return new InvoiceCreationFailedEvent
            {
                ShipmentId = command.ShipmentId,
                Reasons = new[] { ex.Message }
            };
        }
    }
}

