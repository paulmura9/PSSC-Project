using Microsoft.EntityFrameworkCore;
using Ordering.Domain.Models;
using Ordering.Domain.Operations;

namespace Ordering.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of product repository
/// Validates stock quantities are between 0 and 100,000
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly OrderingDbContext _dbContext;

    public ProductRepository(OrderingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProductInfo?> GetProductByNameAsync(string name, CancellationToken ct = default)
    {
        var entity = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.Name == name && p.IsActive, ct);

        if (entity == null)
            return null;

        return new ProductInfo(
            entity.Id,
            entity.Name,
            entity.Description,
            entity.Category,
            entity.Price,
            entity.StockQuantity
        );
    }

    public async Task<bool> ReserveStockAsync(string productName, int quantity, CancellationToken ct = default)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.Name == productName && p.IsActive, ct);

        if (product == null || product.StockQuantity < quantity)
            return false;

        // Validate new stock quantity using Value Object
        var newStockValue = product.StockQuantity - quantity;
        
        if (!StockQuantity.IsValid(newStockValue))
        {
            // Stock would go below 0 or above 10,000
            return false;
        }

        product.StockQuantity = newStockValue;
        await _dbContext.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> RestoreStockAsync(string productName, int quantity, CancellationToken ct = default)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.Name == productName && p.IsActive, ct);

        if (product == null)
            return false;

        // Validate new stock quantity using Value Object
        var newStockValue = product.StockQuantity + quantity;
        
        if (!StockQuantity.IsValid(newStockValue))
        {
            // Stock would exceed 10,000
            return false;
        }

        product.StockQuantity = newStockValue;
        await _dbContext.SaveChangesAsync(ct);
        return true;
    }
}

