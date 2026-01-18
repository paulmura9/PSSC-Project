namespace Ordering.Domain.Models;

/// <summary>
/// Value Object representing a postal code
/// </summary>
public sealed record PostalCode
{
    public string Value { get; }

    private PostalCode(string value)
    {
        Value = value;
    }

    public static PostalCode Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Postal code is required", nameof(value));
        
        var cleanValue = value.Trim();
        
        if (cleanValue.Length < 5 || cleanValue.Length > 6)
            throw new ArgumentException("Postal code must be 5-6 digits", nameof(value));
        
        if (!cleanValue.All(char.IsDigit))
            throw new ArgumentException("Postal code must contain only digits", nameof(value));

        return new PostalCode(cleanValue);
    }

    public override string ToString() => Value;
    public static implicit operator string(PostalCode code) => code.Value;
}

