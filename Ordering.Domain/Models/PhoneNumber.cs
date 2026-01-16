using System.Text.RegularExpressions;
using Ordering.Domain.Exceptions;

namespace Ordering.Domain.Models;

/// <summary>
/// Value object representing a valid phone number
/// </summary>
public record PhoneNumber
{
    public const string Pattern = @"^\+?[0-9]{10,15}$";
    public static readonly Regex ValidPattern = new(Pattern);

    public string Value { get; }

    internal PhoneNumber(string value)
    {
        if (IsValid(value))
        {
            Value = value;
        }
        else
        {
            throw new InvalidPhoneNumberException($"Invalid phone number: {value}");
        }
    }

    private static bool IsValid(string stringValue) => ValidPattern.IsMatch(stringValue);

    public override string ToString() => Value;

    public static bool TryParse(string stringValue, out PhoneNumber? phoneNumber)
    {
        phoneNumber = null;

        if (IsValid(stringValue))
        {
            phoneNumber = new PhoneNumber(stringValue);
            return true;
        }

        return false;
    }
}

