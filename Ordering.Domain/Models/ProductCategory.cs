namespace Ordering.Domain.Models;

/// <summary>
/// Value Object representing a product category
/// </summary>
public sealed record ProductCategory
{
    public string Value { get; }

    private ProductCategory(string value)
    {
        Value = value;
    }

    public static ProductCategory Create(string value)
    {
        if (value.Length > 100)
            throw new ArgumentException("Product category cannot exceed 100 characters", nameof(value));

        return new ProductCategory(value?.Trim() ?? string.Empty);
    }

    public override string ToString() => Value;

    public static implicit operator string(ProductCategory category) => category.Value;
}

