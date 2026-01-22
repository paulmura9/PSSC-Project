using SharedKernel;
using Invoicing.Exceptions;
using static Invoicing.Models.Invoice;

namespace Invoicing.Operations;

/// <summary>
/// Base class for invoice operations with state dependency (SYNC - pure transformations)
/// Extends DomainOperation from SharedKernel
/// Only CreatedInvoice is used in Transform - CalculateVatOperation.OnCreated()
/// </summary>
/// <typeparam name="TState">The state/dependency type for the operation</typeparam>
public abstract class InvoiceOperation<TState> : DomainOperation<IInvoice, TState, IInvoice>
    where TState : class
{
    public override IInvoice Transform(IInvoice invoice, TState? state) => invoice switch
    {
        CreatedInvoice created => OnCreated(created, state),
        VatCalculatedInvoice vatCalculated => OnVatCalculated(vatCalculated, state),
        _ => throw new InvalidInvoiceStateException(invoice.GetType().Name)
    };

    protected virtual IInvoice OnCreated(CreatedInvoice invoice, TState? state) => invoice;
    protected virtual IInvoice OnVatCalculated(VatCalculatedInvoice invoice, TState? state) => invoice;
}

/// <summary>
/// Base class for invoice operations without state dependency (SYNC - pure transformations)
/// </summary>
public abstract class InvoiceOperation : InvoiceOperation<object>
{
    public IInvoice Transform(IInvoice invoice) => Transform(invoice, null);

    protected sealed override IInvoice OnCreated(CreatedInvoice invoice, object? state) => OnCreated(invoice);
    protected virtual IInvoice OnCreated(CreatedInvoice invoice) => invoice;

    protected sealed override IInvoice OnVatCalculated(VatCalculatedInvoice invoice, object? state) => OnVatCalculated(invoice);
    protected virtual IInvoice OnVatCalculated(VatCalculatedInvoice invoice) => invoice;
    
}
