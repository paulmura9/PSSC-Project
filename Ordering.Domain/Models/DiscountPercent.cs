namespace Ordering.Domain.Models;

/// <summary>
/// Value Object for discount percentage
/// Validates: 0 to 100
/// </summary>
public sealed record DiscountPercent
{
    public const int MinValue = 0;
    public const int MaxValue = 100;
    
    public int Value { get; }

    private DiscountPercent(int value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new DiscountPercent with validation
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">If value is outside 0-100 range</exception>
    public static DiscountPercent Create(int value)
    {
        if (value < MinValue || value > MaxValue)
            throw new ArgumentOutOfRangeException(
                nameof(value), 
                $"Discount percent must be between {MinValue} and {MaxValue}. Got: {value}");
        
        return new DiscountPercent(value);
    }

    /// <summary>
    /// Calculates discount amount from a subtotal
    /// </summary>
    public decimal CalculateDiscount(decimal subtotal)
    {
        return Math.Round(subtotal * Value / 100m, 2);
    }

    public override string ToString() => $"{Value}%";

    public static implicit operator int(DiscountPercent percent) => percent.Value;
}

