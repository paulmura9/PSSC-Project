namespace Invoicing.Exceptions;

/// <summary>
/// Exception thrown when invoice is in an invalid/unexpected state
/// </summary>
public class InvalidInvoiceStateException : Exception
{
    public InvalidInvoiceStateException(string stateName)
        : base($"Invalid invoice state: {stateName}")
    {
    }

    public InvalidInvoiceStateException(string stateName, Exception innerException)
        : base($"Invalid invoice state: {stateName}", innerException)
    {
    }
}
