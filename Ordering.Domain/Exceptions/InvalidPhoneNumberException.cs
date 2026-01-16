namespace Ordering.Domain.Exceptions;

/// <summary>
/// Exception thrown when a phone number is invalid
/// </summary>
public class InvalidPhoneNumberException : Exception
{
    public InvalidPhoneNumberException(string message) : base(message)
    {
    }

    public InvalidPhoneNumberException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

