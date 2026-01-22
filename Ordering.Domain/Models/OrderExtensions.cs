using Ordering.Domain.Events;

namespace Ordering.Domain.Models;

/// <summary>
/// Extension methods for Order state conversions
/// </summary>
public static class OrderExtensions
{
    /// <summary>
    /// Converts an order state to the appropriate event
    /// </summary>
    public static IOrderPlacedEvent ToEvent(this Order.IOrder order) =>
        order switch
        {
            Order.InvalidOrder invalid => new OrderPlaceFailedEvent(invalid.Reasons),
            Order.PersistedOrder persisted => new OrderPlacedEvent(
                persisted.OrderId,
                persisted.UserId.Value,
                persisted.PremiumSubscription,
                persisted.Subtotal,
                persisted.DiscountAmount,
                persisted.Total,
                persisted.VoucherCode?.Value,
                persisted.Lines,
                persisted.Street?.Value ?? string.Empty,
                persisted.City?.Value ?? string.Empty,
                persisted.PostalCode?.Value ?? string.Empty,
                persisted.Phone.Value,
                persisted.Email?.Value,
                persisted.CreatedAt),
            Order.PublishedOrder published => new OrderPlacedEvent(
                published.OrderId,
                published.UserId.Value,
                published.PremiumSubscription,
                published.Subtotal,
                published.DiscountAmount,
                published.Total,
                published.VoucherCode?.Value,
                published.Lines,
                published.Street?.Value ?? string.Empty,
                published.City?.Value ?? string.Empty,
                published.PostalCode?.Value ?? string.Empty,
                published.Phone.Value,
                published.Email?.Value,
                published.PublishedAt),
            _ => new OrderPlaceFailedEvent($"Unexpected order state: {order.GetType().Name}")
        };
}

