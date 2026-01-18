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
            Order.UnvalidatedOrder => new OrderPlaceFailedEvent("Unexpected unvalidated state"),
            Order.ValidatedOrder => new OrderPlaceFailedEvent("Unexpected validated state"),
            Order.PricedOrder => new OrderPlaceFailedEvent("Unexpected priced state"),
            Order.PersistableOrder => new OrderPlaceFailedEvent("Unexpected persistable state"),
            Order.InvalidOrder invalid => new OrderPlaceFailedEvent(invalid.Reasons),
            Order.PersistedOrder persisted => new OrderPlacedEvent(
                persisted.OrderId,
                persisted.UserId,
                persisted.PremiumSubscription,
                persisted.Subtotal,
                persisted.DiscountAmount,
                persisted.Total,
                persisted.VoucherCode,
                persisted.Lines,
                persisted.Street,
                persisted.City,
                persisted.PostalCode,
                persisted.Phone,
                persisted.Email,
                persisted.CreatedAt),
            Order.PublishedOrder published => new OrderPlacedEvent(
                published.OrderId,
                published.UserId,
                published.PremiumSubscription,
                published.Subtotal,
                published.DiscountAmount,
                published.Total,
                published.VoucherCode,
                published.Lines,
                published.Street,
                published.City,
                published.PostalCode,
                published.Phone,
                published.Email,
                published.PublishedAt),
            _ => throw new NotImplementedException($"Unknown order state: {order.GetType().Name}")
        };
}

