using System.ComponentModel.DataAnnotations;

namespace Ordering.Domain.Models;

/// <summary>
/// Value Object for stock quantity
/// Validates that stock is between 0 and 10,000
/// </summary>
public sealed record StockQuantity
{
    public const int MinValue = 0;
    public const int MaxValue = 10000;
    
    public int Value { get; }

    private StockQuantity(int value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new StockQuantity with validation
    /// </summary>
    /// <param name="value">Stock quantity (0-10000)</param>
    /// <exception cref="ArgumentOutOfRangeException">If value is outside valid range</exception>
    public static StockQuantity Create(int value)
    {
        if (value < MinValue || value > MaxValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value), 
                $"Stock quantity must be between {MinValue} and {MaxValue}. Got: {value}");
        }
        
        return new StockQuantity(value);
    }

    /// <summary>
    /// Tries to create a StockQuantity, returns null if invalid
    /// </summary>
    public static StockQuantity? TryCreate(int value)
    {
        if (value < MinValue || value > MaxValue)
            return null;
        
        return new StockQuantity(value);
    }

    /// <summary>
    /// Validates if a value is a valid stock quantity
    /// </summary>
    public static bool IsValid(int value) => value >= MinValue && value <= MaxValue;

    /// <summary>
    /// Subtracts quantity (for reserving stock)
    /// </summary>
    /// <exception cref="InvalidOperationException">If result would be negative</exception>
    public StockQuantity Subtract(int quantity)
    {
        var newValue = Value - quantity;
        if (newValue < MinValue)
        {
            throw new InvalidOperationException(
                $"Cannot subtract {quantity} from stock of {Value}. Would result in negative stock.");
        }
        
        return Create(newValue);
    }

    /// <summary>
    /// Adds quantity (for restocking or returns)
    /// </summary>
    /// <exception cref="InvalidOperationException">If result exceeds maximum</exception>
    public StockQuantity Add(int quantity)
    {
        var newValue = Value + quantity;
        if (newValue > MaxValue)
        {
            throw new InvalidOperationException(
                $"Cannot add {quantity} to stock of {Value}. Would exceed maximum of {MaxValue}.");
        }
        
        return Create(newValue);
    }

    /// <summary>
    /// Checks if there's enough stock to fulfill a quantity
    /// </summary>
    public bool HasEnough(int quantity) => Value >= quantity;

    public override string ToString() => Value.ToString();

    public static implicit operator int(StockQuantity stock) => stock.Value;
}

/// <summary>
/// Validation attribute for stock quantity in DTOs
/// </summary>
public class StockQuantityRangeAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is int intValue)
        {
            if (intValue < StockQuantity.MinValue || intValue > StockQuantity.MaxValue)
            {
                return new ValidationResult(
                    $"Stock quantity must be between {StockQuantity.MinValue} and {StockQuantity.MaxValue}");
            }
        }
        
        return ValidationResult.Success;
    }
}

