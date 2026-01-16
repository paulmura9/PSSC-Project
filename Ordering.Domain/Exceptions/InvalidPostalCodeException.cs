namespace Ordering.Domain.Exceptions;

/// <summary>
/// Exception thrown when a postal code is invalid
/// </summary>
public class InvalidPostalCodeException : Exception
{
    public InvalidPostalCodeException(string message) : base(message)
    {
    }

    public InvalidPostalCodeException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

