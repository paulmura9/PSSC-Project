using Ordering.Domain.Models;

namespace Ordering.Domain.Repositories;

/// <summary>
/// Interface for product repository operations
/// Stock quantity is validated to be between 0 and 100,000
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Gets a product by name
    /// </summary>
    Task<ProductInfo?> GetProductByNameAsync(string name, CancellationToken ct = default);
    
    /// <summary>
    /// Reserves stock for a product (decreases stock quantity)
    /// Validates that resulting stock stays between 0 and 10,000
    /// </summary>
    Task<bool> ReserveStockAsync(string productName, int quantity, CancellationToken ct = default);

    /// <summary>
    /// Restores stock for a product (increases stock quantity)
    /// Used when order is cancelled or returned
    /// Validates that resulting stock stays between 0 and 10,000
    /// </summary>
    Task<bool> RestoreStockAsync(string productName, int quantity, CancellationToken ct = default);
}


