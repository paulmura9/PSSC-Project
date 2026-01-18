namespace Ordering.Domain.Operations;

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

