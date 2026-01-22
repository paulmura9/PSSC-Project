namespace Invoicing.Models;

/// <summary>
/// Value Object representing money amount in invoicing
/// Data comes pre-validated from Ordering, so minimal validation here
/// </summary>
public sealed record Money
{
    public decimal Value { get; }

    public Money(decimal value)
    {
        Value = Math.Round(value, 2);
    }

    public Money Add(Money other) => new Money(Value + other.Value);
    
    public Money Subtract(Money other) => new Money(Value - other.Value);

    public override string ToString() => $"{Value:C}";
}

