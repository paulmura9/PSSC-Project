namespace Shipment.Domain.Models;

/// <summary>
/// Value Object representing a price/money amount
/// </summary>
public sealed record Money
{
    public decimal Value { get; }

    public Money(decimal value)
    {
        if (value < 0)
            throw new ArgumentException("Money cannot be negative", nameof(value));

        Value = Math.Round(value, 2);
    }

    public static Money Zero => new(0);

    public Money Add(Money other) => new(Value + other.Value);
    public Money Multiply(int quantity) => new(Value * quantity);

    public override string ToString() => $"{Value:C}";
}

