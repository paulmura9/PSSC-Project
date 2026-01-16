using System.Text.RegularExpressions;
using Ordering.Domain.Exceptions;

namespace Ordering.Domain.Models;

/// <summary>
/// Value object representing a valid CVV (3 digits)
/// </summary>
public record Cvv
{
    public const string Pattern = @"^[0-9]{3}$";
    public static readonly Regex ValidPattern = new(Pattern);

    public string Value { get; }

    internal Cvv(string value)
    {
        if (IsValid(value))
        {
            Value = value;
        }
        else
        {
            throw new InvalidCvvException($"Invalid CVV: {value}. Must be exactly 3 digits.");
        }
    }

    private static bool IsValid(string stringValue) => ValidPattern.IsMatch(stringValue);

    public override string ToString() => "***"; // Never expose CVV

    public static bool TryParse(string stringValue, out Cvv? cvv)
    {
        cvv = null;

        if (IsValid(stringValue))
        {
            cvv = new Cvv(stringValue);
            return true;
        }

        return false;
    }
}

