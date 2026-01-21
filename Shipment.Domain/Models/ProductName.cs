namespace Shipment.Domain.Models;

/// <summary>
/// Value Object representing a product name in shipment
/// Data comes pre-validated from Ordering
/// </summary>
public sealed record ProductName
{
    public string Value { get; }

    public ProductName(string value)
    {
        Value = value ?? string.Empty;
    }

    public override string ToString() => Value;
}
