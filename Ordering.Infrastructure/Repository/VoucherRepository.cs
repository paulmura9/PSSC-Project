using Microsoft.EntityFrameworkCore;
using Ordering.Domain.Models;
using Ordering.Domain.Operations;
using Ordering.Infrastructure.Persistence;

namespace Ordering.Infrastructure.Repository;

/// <summary>
/// EF Core implementation of IVoucherRepository
/// Provides thread-safe voucher consumption
/// </summary>
public class VoucherRepository : IVoucherRepository
{
    private readonly OrderingDbContext _context;

    public VoucherRepository(OrderingDbContext context)
    {
        _context = context;
    }

    //SELECT * FROM Vouchers 
    public async Task<VoucherData?> GetActiveByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        
        var entity = await _context.Vouchers
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Code == normalizedCode && v.IsActive, cancellationToken);

        if (entity == null)
            return null;

        return new VoucherData
        {
            VoucherId = entity.VoucherId,
            Code = entity.Code,
            DiscountPercent = entity.DiscountPercent,
            IsActive = entity.IsActive,
            ValidFrom = entity.ValidFrom,
            ValidTo = entity.ValidTo,
            MaxUses = entity.MaxUses
        };
    }

    //UPDATE scade
    public async Task<bool> TryConsumeAsync(Guid voucherId, CancellationToken cancellationToken = default)
    {
        // Use raw SQL for atomic update with concurrency check
        // Decrement MaxUses (if not null and > 0)
        // Only succeeds if MaxUses is null (unlimited) OR MaxUses > 0
        var rowsAffected = await _context.Database.ExecuteSqlRawAsync(
            @"UPDATE [ordering].[Vouchers] 
              SET MaxUses = CASE WHEN MaxUses IS NOT NULL THEN MaxUses - 1 ELSE MaxUses END
              WHERE VoucherId = {0} 
                AND IsActive = 1 
                AND (MaxUses IS NULL OR MaxUses > 0)",
            new object[] { voucherId },
            cancellationToken);

        return rowsAffected == 1;
    }
}

