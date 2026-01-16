namespace Ordering.Domain.Exceptions;

/// <summary>
/// Exception thrown when a CVV is invalid
/// </summary>
public class InvalidCvvException : Exception
{
    public InvalidCvvException(string message) : base(message)
    {
    }

    public InvalidCvvException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

