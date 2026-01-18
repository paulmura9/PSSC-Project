namespace Ordering.Domain.Models;

/// <summary>
/// Value Object representing a product description
/// </summary>
public sealed record ProductDescription
{
    public string Value { get; }

    private ProductDescription(string value)
    {
        Value = value;
    }

    public static ProductDescription Create(string value)
    {
        if (value.Length > 1000)
            throw new ArgumentException("Product description cannot exceed 1000 characters", nameof(value));

        return new ProductDescription(value?.Trim() ?? string.Empty);
    }

    public override string ToString() => Value;

    public static implicit operator string(ProductDescription description) => description.Value;
}

