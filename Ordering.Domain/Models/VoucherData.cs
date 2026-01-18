namespace Ordering.Domain.Models;

/// <summary>
/// Data record for voucher information from repository
/// </summary>
public record VoucherData
{
    public Guid VoucherId { get; init; }
    public string Code { get; init; } = string.Empty;
    public int DiscountPercent { get; init; }
    public bool IsActive { get; init; }
    public DateTime? ValidFrom { get; init; }
    public DateTime? ValidTo { get; init; }
    /// <summary>
    /// Remaining uses for this voucher. Decremented on each use.
    /// NULL means unlimited uses.
    /// </summary>
    public int? MaxUses { get; init; }

    /// <summary>
    /// Checks if voucher is currently valid
    /// </summary>
    public bool IsValid(DateTime utcNow)
    {
        if (!IsActive)
            return false;

        if (ValidFrom.HasValue && utcNow < ValidFrom.Value)
            return false;

        if (ValidTo.HasValue && utcNow > ValidTo.Value)
            return false;

        // MaxUses = remaining uses, if 0 or less, voucher is exhausted
        if (MaxUses.HasValue && MaxUses.Value <= 0)
            return false;

        return true;
    }

    /// <summary>
    /// Gets validation error message if voucher is invalid
    /// </summary>
    public string? GetValidationError(DateTime utcNow)
    {
        if (!IsActive)
            return "Voucher is not active";

        if (ValidFrom.HasValue && utcNow < ValidFrom.Value)
            return $"Voucher is not yet valid. Valid from: {ValidFrom.Value:yyyy-MM-dd}";

        if (ValidTo.HasValue && utcNow > ValidTo.Value)
            return $"Voucher has expired. Valid until: {ValidTo.Value:yyyy-MM-dd}";

        if (MaxUses.HasValue && MaxUses.Value <= 0)
            return "Voucher has no remaining uses";

        return null;
    }
}

