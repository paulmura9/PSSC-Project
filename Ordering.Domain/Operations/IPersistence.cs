using static Ordering.Domain.Models.Order;

namespace Ordering.Domain.Operations;

/// <summary>
/// Interface for order persistence operations
/// </summary>
public interface IPersistence
{
    /// <summary>
    /// Saves an order to the database and returns the generated order ID
    /// </summary>
    Task<Guid> SaveOrderAsync(PersistableOrder order, CancellationToken cancellationToken);
}
