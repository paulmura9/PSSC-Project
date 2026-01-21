using static Invoicing.Models.Invoice;

namespace Invoicing.Operations;

/// <summary>
/// Base class for invoice operations following Lab-style DDD pattern (SYNC - pure transformations)
/// Async is only used for DB persistence and Service Bus publishing in workflow
/// </summary>
public abstract class InvoiceOperation : InvoiceOperationWithState<object>
{
    public IInvoice Transform(IInvoice invoice)
    {
        return Transform(invoice, null!);
    }

    protected virtual IInvoice OnCreated(CreatedInvoice invoice) => invoice;
    protected virtual IInvoice OnCalculated(CalculatedInvoice invoice) => invoice;
    protected virtual IInvoice OnPersisted(PersistedInvoice invoice) => invoice;
    protected virtual IInvoice OnPublished(PublishedInvoice invoice) => invoice;
    protected virtual IInvoice OnCancelled(CancelledInvoice invoice) => invoice;
    protected virtual IInvoice OnInvalid(InvalidInvoice invoice) => invoice;

    protected override IInvoice OnCreated(CreatedInvoice invoice, object? state) => OnCreated(invoice);
    protected override IInvoice OnCalculated(CalculatedInvoice invoice, object? state) => OnCalculated(invoice);
    protected override IInvoice OnPersisted(PersistedInvoice invoice, object? state) => OnPersisted(invoice);
    protected override IInvoice OnPublished(PublishedInvoice invoice, object? state) => OnPublished(invoice);
    protected override IInvoice OnCancelled(CancelledInvoice invoice, object? state) => OnCancelled(invoice);
    protected override IInvoice OnInvalid(InvalidInvoice invoice, object? state) => OnInvalid(invoice);
}

/// <summary>
/// Base class for invoice operations that need external state/dependencies (SYNC - pure transformations)
/// </summary>
public abstract class InvoiceOperationWithState<TState> where TState : class
{
    public IInvoice Transform(IInvoice invoice, TState? state)
    {
        return invoice switch
        {
            CreatedInvoice created => OnCreated(created, state),
            CalculatedInvoice calculated => OnCalculated(calculated, state),
            PersistedInvoice persisted => OnPersisted(persisted, state),
            PublishedInvoice published => OnPublished(published, state),
            CancelledInvoice cancelled => OnCancelled(cancelled, state),
            InvalidInvoice invalid => OnInvalid(invalid, state),
            _ => invoice
        };
    }

    protected virtual IInvoice OnCreated(CreatedInvoice invoice, TState? state) => invoice;
    protected virtual IInvoice OnCalculated(CalculatedInvoice invoice, TState? state) => invoice;
    protected virtual IInvoice OnPersisted(PersistedInvoice invoice, TState? state) => invoice;
    protected virtual IInvoice OnPublished(PublishedInvoice invoice, TState? state) => invoice;
    protected virtual IInvoice OnCancelled(CancelledInvoice invoice, TState? state) => invoice;
    protected virtual IInvoice OnInvalid(InvalidInvoice invoice, TState? state) => invoice;
}

