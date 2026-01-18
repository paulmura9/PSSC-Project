namespace Shipment.Domain.Models;

/// <summary>
/// Value Object representing a product description
/// </summary>
public sealed record ProductDescription
{
    public string Value { get; }

    public ProductDescription(string value)
    {
        Value = value ?? string.Empty;
    }

    public override string ToString() => Value;
}

