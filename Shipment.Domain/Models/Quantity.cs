namespace Shipment.Domain.Models;

/// <summary>
/// Value Object representing a quantity
/// </summary>
public sealed record Quantity
{
    public int Value { get; }

    public Quantity(int value)
    {
        if (value <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(value));
        
        if (value > 10000)
            throw new ArgumentException("Quantity cannot exceed 10,000", nameof(value));

        Value = value;
    }

    public override string ToString() => Value.ToString();
}

