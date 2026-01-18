namespace Ordering.Domain.Models;

/// <summary>
/// Value Object representing a product name
/// </summary>
public sealed record ProductName
{
    public string Value { get; }

    private ProductName(string value)
    {
        Value = value;
    }

    public static ProductName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Product name cannot be empty", nameof(value));
        
        if (value.Length < 2 || value.Length > 200)
            throw new ArgumentException("Product name must be between 2 and 200 characters", nameof(value));

        return new ProductName(value.Trim());
    }

    public override string ToString() => Value;

    public static implicit operator string(ProductName name) => name.Value;
}

