namespace Ordering.Domain.Models;

/// <summary>
/// Value Object representing a product quantity
/// </summary>
public sealed record Quantity
{
    public int Value { get; }

    private Quantity(int value)
    {
        Value = value;
    }

    public static Quantity Create(int value)
    {
        if (value < 1)
            throw new ArgumentException("Quantity must be at least 1", nameof(value));
        
        if (value > 10000)
            throw new ArgumentException("Quantity cannot exceed 10,000", nameof(value));

        return new Quantity(value);
    }

    public override string ToString() => Value.ToString();

    public static implicit operator int(Quantity quantity) => quantity.Value;
}

