namespace Ordering.Domain.Models;

/// <summary>
/// Value Object representing a phone number
/// </summary>
public sealed record PhoneNumber
{
    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static PhoneNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Phone number is required", nameof(value));
        
        var cleanValue = value.Trim().Replace(" ", "").Replace("-", "");
        
        // Romanian phone format: 07xxxxxxxx or +407xxxxxxxx
        if (cleanValue.StartsWith("+40"))
            cleanValue = "0" + cleanValue[3..];
        
        if (cleanValue.Length != 10)
            throw new ArgumentException("Phone number must be 10 digits", nameof(value));
        
        if (!cleanValue.StartsWith("07"))
            throw new ArgumentException("Phone number must start with 07", nameof(value));
        
        if (!cleanValue.All(char.IsDigit))
            throw new ArgumentException("Phone number must contain only digits", nameof(value));

        return new PhoneNumber(cleanValue);
    }

    public override string ToString() => Value;
    public static implicit operator string(PhoneNumber phone) => phone.Value;
}

