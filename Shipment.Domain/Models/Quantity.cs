namespace Shipment.Domain.Models;

/// <summary>
/// Value Object representing a quantity in shipment
/// Data comes pre-validated from Ordering
/// </summary>
public sealed record Quantity
{
    public int Value { get; }

    public Quantity(int value)
    {

        Value = value;
    }

    public override string ToString() => Value.ToString();
}

