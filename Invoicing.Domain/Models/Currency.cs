namespace Invoicing.Models;

/// <summary>
/// Value Object representing supported currencies for invoice display
/// Database stores values in RON only. EUR is derived for presentation.
/// </summary>
public sealed record Currency
{
    public string Value { get; }

    public Currency(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Currency is required", nameof(value));

        var trimmed = value.Trim().ToUpperInvariant();

        // Direct case-insensitive comparison to avoid static initialization issues
        if (trimmed.Equals("RON", StringComparison.OrdinalIgnoreCase))
        {
            Value = "RON";
        }
        else if (trimmed.Equals("EUR", StringComparison.OrdinalIgnoreCase))
        {
            Value = "EUR";
        }
        else
        {
            throw new ArgumentException($"Invalid currency: {value}. Allowed values: RON, EUR", nameof(value));
        }
    }

    public bool IsRon => Value == "RON";
    public bool IsEur => Value == "EUR";

    public static Currency Default() => new("RON");

    public static bool TryParse(string? value, out Currency? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        try
        {
            result = new Currency(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public override string ToString() => Value;
}

