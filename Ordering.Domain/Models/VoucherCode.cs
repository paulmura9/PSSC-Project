namespace Ordering.Domain.Models;

/// <summary>
/// Value Object for voucher code
/// Validates: not empty, max 64 characters, normalized to uppercase
/// </summary>
public sealed record VoucherCode
{
    public const int MaxLength = 64;
    
    public string Value { get; }

    private VoucherCode(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new VoucherCode with validation
    /// </summary>
    /// <exception cref="ArgumentException">If code is invalid</exception>
    public static VoucherCode Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Voucher code cannot be empty", nameof(code));
        
        var normalized = code.Trim().ToUpperInvariant();
        
        if (normalized.Length > MaxLength)
            throw new ArgumentException($"Voucher code cannot exceed {MaxLength} characters", nameof(code));
        
        return new VoucherCode(normalized);
    }

    /// <summary>
    /// Tries to create a VoucherCode, returns null if invalid
    /// </summary>
    public static VoucherCode? TryCreate(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;
        
        var normalized = code.Trim().ToUpperInvariant();
        
        if (normalized.Length > MaxLength)
            return null;
        
        return new VoucherCode(normalized);
    }

    public override string ToString() => Value;

    public static implicit operator string(VoucherCode code) => code.Value;
}

