namespace Shipment.Domain.Models;

/// <summary>
/// Value Object representing a product name in shipment
/// </summary>
public sealed record ProductName
{
    public string Value { get; }

    public ProductName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Product name is required", nameof(value));
        
        if (value.Length > 200)
            throw new ArgumentException("Product name cannot exceed 200 characters", nameof(value));

        Value = value;
    }

    public override string ToString() => Value;
}

