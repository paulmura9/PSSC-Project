 namespace Shipment.Domain.Models;

/// <summary>
/// Value Object representing a product category
/// Used for VAT calculation in Invoicing
/// </summary>
public sealed record ProductCategory
{
    public string Value { get; }

    public ProductCategory(string value)
    {
        Value = value ?? "Other";
    }

    public override string ToString() => Value;
}

