namespace Invoicing.Models;

/// <summary>
/// Value Object representing a product description in invoice
/// Data comes pre-validated from Ordering
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

