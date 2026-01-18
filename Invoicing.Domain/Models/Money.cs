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
        // Data is pre-validated from Ordering
        Value = Math.Round(value, 2);
    }

    public static Money Zero => new(0);

    public Money Add(Money other) => new(Value + other.Value);
    public Money Subtract(Money other) => new(Value - other.Value);
    public Money Multiply(decimal factor) => new(Value * factor);

    /// <summary>
    /// Calculate tax (VAT) at given percentage
    /// </summary>
    public Money CalculateTax(decimal taxRate) => new(Value * taxRate / 100);

    public override string ToString() => $"{Value:C}";
}

