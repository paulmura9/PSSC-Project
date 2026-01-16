using Ordering.Domain.Models;
using SharedKernel;
using static Ordering.Domain.Models.Order;

namespace Ordering.Domain.Events;

/// <summary>
/// Static class containing order placed events (similar to ExamPublishedEvent in lab)
/// </summary>
public static class OrderPlacedEvent
{
    public interface IOrderPlacedEvent { }

    /// <summary>
    /// Event indicating that an order was successfully placed
    /// </summary>
    public record OrderPlaceSucceededEvent : IntegrationEvent, IOrderPlacedEvent
    {
        public Guid OrderId { get; }
        public Guid UserId { get; }
        public decimal TotalPrice { get; }
        public IReadOnlyCollection<ValidatedOrderLine> Lines { get; }

        internal OrderPlaceSucceededEvent(
            Guid orderId,
            Guid userId,
            decimal totalPrice,
            IReadOnlyCollection<ValidatedOrderLine> lines,
            DateTime occurredAt) : base(Guid.NewGuid(), occurredAt)
        {
            OrderId = orderId;
            UserId = userId;
            TotalPrice = totalPrice;
            Lines = lines;
        }
    }

    /// <summary>
    /// Event indicating that order placement failed
    /// </summary>
    public record OrderPlaceFailedEvent : IOrderPlacedEvent
    {
        public IEnumerable<string> Reasons { get; }

        internal OrderPlaceFailedEvent(string reason)
        {
            Reasons = new[] { reason };
        }

        internal OrderPlaceFailedEvent(IEnumerable<string> reasons)
        {
            Reasons = reasons;
        }
    }

    /// <summary>
    /// Converts an order state to the appropriate event
    /// </summary>
    public static IOrderPlacedEvent ToEvent(this IOrder order) =>
        order switch
        {
            UnvalidatedOrder => new OrderPlaceFailedEvent("Unexpected unvalidated state"),
            ValidatedOrder => new OrderPlaceFailedEvent("Unexpected validated state"),
            PricedOrder => new OrderPlaceFailedEvent("Unexpected priced state"),
            InvalidOrder invalidOrder => new OrderPlaceFailedEvent(invalidOrder.Reasons),
            PersistedOrder persistedOrder => new OrderPlaceSucceededEvent(
                persistedOrder.OrderId,
                persistedOrder.UserId,
                persistedOrder.TotalPrice,
                persistedOrder.Lines,
                persistedOrder.CreatedAt),
            PublishedOrder publishedOrder => new OrderPlaceSucceededEvent(
                publishedOrder.OrderId,
                publishedOrder.UserId,
                publishedOrder.TotalPrice,
                publishedOrder.Lines,
                publishedOrder.PublishedAt),
            _ => throw new NotImplementedException()
        };
}

