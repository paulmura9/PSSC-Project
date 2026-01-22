using SharedKernel;
using SharedKernel.Messaging;
using Invoicing.Models;
using static Invoicing.Models.Invoice;

namespace Invoicing.Operations;

/// <summary>
/// Publishes a PersistedInvoice to the event bus and returns PublishedInvoice
/// PersistedInvoice -> PublishedInvoice
/// ASYNC - requires I/O (Service Bus)
/// </summary>
public class PublishInvoiceOperation
{
    private readonly IEventBus _eventBus;

    public PublishInvoiceOperation(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task<IInvoice> ExecuteAsync(PersistedInvoice invoice, string paymentStatus, CancellationToken cancellationToken = default)
    {
        //VO->string(dto)
        var stateChangedEvent = new InvoiceStateChangedEvent
        {
            InvoiceId = invoice.InvoiceId,
            InvoiceNumber = invoice.InvoiceNumber.Value,
            OrderId = invoice.OrderId,
            ShipmentId = invoice.ShipmentId,
            UserId = invoice.UserId,
            TrackingNumber = invoice.TrackingNumber.Value,
            InvoiceState = "Created",
            SubTotal = invoice.SubTotal.Value,
            Tax = invoice.Tax.Value,
            TotalAmount = invoice.TotalAmount.Value,
            PaymentStatus = paymentStatus,
            InvoiceDate = invoice.InvoiceDate,
            DueDate = invoice.DueDate,
            Lines = invoice.Lines.Select(l => new LineItemDto(
                l.Name.Value,
                l.Description.Value,
                l.Category.ToString(),
                l.Quantity.Value,
                l.UnitPrice.Value,
                l.LineTotalWithVat.Value
            )).ToList(),
            Currency = invoice.DisplayCurrency.Value,
            TotalInRon = invoice.TotalInRon,
            TotalInEur = invoice.TotalInEur,
            OccurredAt = DateTime.UtcNow
        };

        await _eventBus.PublishAsync(TopicNames.Invoices, stateChangedEvent, cancellationToken);

        return new PublishedInvoice(invoice, DateTime.UtcNow);
    }
}

