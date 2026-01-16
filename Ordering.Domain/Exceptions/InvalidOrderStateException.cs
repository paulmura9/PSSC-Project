namespace Ordering.Domain.Exceptions;

/// <summary>
/// Exception thrown when an operation encounters an unexpected order state
/// </summary>
public class InvalidOrderStateException : Exception
{
    public InvalidOrderStateException(string message) : base(message)
    {
    }

    public InvalidOrderStateException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

