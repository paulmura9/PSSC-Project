namespace Ordering.Domain.Models;

/// <summary>
/// Value Object representing a price (unit price or total)
/// </summary>
public sealed record Price
{
    public decimal Value { get; }

    private Price(decimal value)
    {
        Value = value;
    }

    public static Price Create(decimal value)
    {
        if (value < 0)
            throw new ArgumentException("Price cannot be negative", nameof(value));

        return new Price(Math.Round(value, 2));
    }

    public static Price Zero => new(0);

    public Price Multiply(int quantity) => new(Value * quantity);

    public static Price operator +(Price a, Price b) => new(a.Value + b.Value);

    public override string ToString() => Value.ToString("C");

    public static implicit operator decimal(Price price) => price.Value;
}

