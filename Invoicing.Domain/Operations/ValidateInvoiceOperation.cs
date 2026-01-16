using static Invoicing.Models.Invoice;

namespace Invoicing.Operations;

/// <summary>
/// Validates an invoice - transforms UnprocessedInvoice to ValidatedInvoice
/// </summary>
public class ValidateInvoiceOperation
{
    public Task<IInvoice> ExecuteAsync(UnprocessedInvoice invoice, CancellationToken cancellationToken = default)
    {
        var validationErrors = new List<string>();

        if (invoice.ShipmentId == Guid.Empty)
            validationErrors.Add("ShipmentId is required");

        if (invoice.OrderId == Guid.Empty)
            validationErrors.Add("OrderId is required");

        if (invoice.UserId == Guid.Empty)
            validationErrors.Add("UserId is required");

        if (invoice.TotalPrice <= 0)
            validationErrors.Add("TotalPrice must be greater than zero");

        if (!invoice.Lines.Any())
            validationErrors.Add("At least one line item is required");

        if (validationErrors.Any())
        {
            return Task.FromResult<IInvoice>(new InvalidInvoice(invoice.ShipmentId, validationErrors));
        }

        return Task.FromResult<IInvoice>(new ValidatedInvoice(
            invoice.ShipmentId,
            invoice.OrderId,
            invoice.UserId,
            invoice.TrackingNumber,
            invoice.TotalPrice,
            invoice.Lines,
            invoice.ShipmentCreatedAt));
    }
}

