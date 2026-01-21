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


    public override string ToString() => $"{Value:C}";
}

