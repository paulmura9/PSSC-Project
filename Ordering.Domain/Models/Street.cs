namespace Ordering.Domain.Models;

/// <summary>
/// Value Object representing a street address
/// </summary>
public sealed record Street
{
    public string Value { get; }

    private Street(string value)
    {
        Value = value;
    }

    public static Street Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Street is required", nameof(value));
        
        if (value.Length < 5)
            throw new ArgumentException("Street must be at least 5 characters", nameof(value));
        
        if (value.Length > 200)
            throw new ArgumentException("Street cannot exceed 200 characters", nameof(value));

        return new Street(value.Trim());
    }

    public override string ToString() => Value;
    public static implicit operator string(Street street) => street.Value;
}

