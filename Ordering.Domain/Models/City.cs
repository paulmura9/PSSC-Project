namespace Ordering.Domain.Models;

/// <summary>
/// Value Object representing a city name
/// </summary>
public sealed record City
{
    public string Value { get; }

    private City(string value)
    {
        Value = value;
    }

    public static City Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("City is required", nameof(value));
        
        if (value.Length < 2)
            throw new ArgumentException("City must be at least 2 characters", nameof(value));
        
        if (value.Length > 100)
            throw new ArgumentException("City cannot exceed 100 characters", nameof(value));

        return new City(value.Trim());
    }

    public override string ToString() => Value;
    public static implicit operator string(City city) => city.Value;
}

