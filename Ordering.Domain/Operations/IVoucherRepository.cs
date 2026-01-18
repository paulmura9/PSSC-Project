using Ordering.Domain.Models;

namespace Ordering.Domain.Operations;

/// <summary>
/// Repository interface for voucher operations
/// </summary>
public interface IVoucherRepository
{
    /// <summary>
    /// Gets an active voucher by its code
    /// </summary>
    /// <param name="code">Voucher code (case-insensitive)</param>
    /// <returns>Voucher data or null if not found</returns>
    Task<VoucherData?> GetActiveByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempts to consume a voucher use atomically
    /// Increments Uses if MaxUses not exceeded
    /// </summary>
    /// <param name="voucherId">Voucher ID</param>
    /// <returns>True if consumption succeeded, false if voucher exhausted</returns>
    Task<bool> TryConsumeAsync(Guid voucherId, CancellationToken cancellationToken = default);
}

