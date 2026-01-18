using static Invoicing.Models.Invoice;

namespace Invoicing.Operations;

/// <summary>
/// Base class for invoice operations following Lab-style DDD pattern
/// Similar to OrderOperation in Ordering.Domain
/// </summary>
public abstract class InvoiceOperation
{
    public async Task<IInvoice> TransformAsync(IInvoice invoice, CancellationToken cancellationToken = default)
    {
        return invoice switch
        {
            CreatedInvoice created => await OnCreatedAsync(created, cancellationToken),
            CalculatedInvoice calculated => await OnCalculatedAsync(calculated, cancellationToken),
            PersistedInvoice persisted => await OnPersistedAsync(persisted, cancellationToken),
            PublishedInvoice published => await OnPublishedAsync(published, cancellationToken),
            CancelledInvoice cancelled => await OnCancelledAsync(cancelled, cancellationToken),
            InvalidInvoice invalid => await OnInvalidAsync(invalid, cancellationToken),
            _ => throw new InvalidOperationException($"Unknown invoice state: {invoice.GetType().Name}")
        };
    }

    protected virtual Task<IInvoice> OnCreatedAsync(CreatedInvoice invoice, CancellationToken cancellationToken)
        => Task.FromResult<IInvoice>(invoice);

    protected virtual Task<IInvoice> OnCalculatedAsync(CalculatedInvoice invoice, CancellationToken cancellationToken)
        => Task.FromResult<IInvoice>(invoice);

    protected virtual Task<IInvoice> OnPersistedAsync(PersistedInvoice invoice, CancellationToken cancellationToken)
        => Task.FromResult<IInvoice>(invoice);

    protected virtual Task<IInvoice> OnPublishedAsync(PublishedInvoice invoice, CancellationToken cancellationToken)
        => Task.FromResult<IInvoice>(invoice);

    protected virtual Task<IInvoice> OnCancelledAsync(CancelledInvoice invoice, CancellationToken cancellationToken)
        => Task.FromResult<IInvoice>(invoice);

    protected virtual Task<IInvoice> OnInvalidAsync(InvalidInvoice invoice, CancellationToken cancellationToken)
        => Task.FromResult<IInvoice>(invoice);
}

