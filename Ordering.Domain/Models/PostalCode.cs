using System.Text.RegularExpressions;
using Ordering.Domain.Exceptions;

namespace Ordering.Domain.Models;

/// <summary>
/// Value object representing a valid postal code
/// </summary>
public record PostalCode
{
    public const string Pattern = @"^[0-9]{5,6}$";
    public static readonly Regex ValidPattern = new(Pattern);

    public string Value { get; }

    internal PostalCode(string value)
    {
        if (IsValid(value))
        {
            Value = value;
        }
        else
        {
            throw new InvalidPostalCodeException($"Invalid postal code: {value}");
        }
    }

    private static bool IsValid(string stringValue) => ValidPattern.IsMatch(stringValue);

    public override string ToString() => Value;

    public static bool TryParse(string stringValue, out PostalCode? postalCode)
    {
        postalCode = null;

        if (IsValid(stringValue))
        {
            postalCode = new PostalCode(stringValue);
            return true;
        }

        return false;
    }
}

