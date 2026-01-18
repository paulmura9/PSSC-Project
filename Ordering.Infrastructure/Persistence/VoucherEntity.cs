namespace Ordering.Infrastructure.Persistence;

/// <summary>
/// Entity representing a voucher in the database
/// </summary>
public class VoucherEntity
{
    public Guid VoucherId { get; set; }
    public string Code { get; set; } = string.Empty;
    public int DiscountPercent { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    /// <summary>
    /// Remaining uses. Decremented on each use. NULL = unlimited.
    /// </summary>
    public int? MaxUses { get; set; }
}

