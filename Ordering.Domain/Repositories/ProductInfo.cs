namespace Ordering.Domain.Repositories;

/// <summary>
/// Product information returned from repository
/// </summary>
public record ProductInfo(
    Guid Id,
    string Name,
    string Description,
    string Category,
    decimal Price,
    int StockQuantity
);
