namespace Ordering.Domain.Events;

/// <summary>
/// Interface for modify order workflow result events
/// </summary>
public interface IOrderModifiedEvent { }

/// <summary>
/// Event indicating order was modified successfully
/// </summary>
public record OrderModifiedSucceededEvent(
    Guid OrderId,
    Guid UserId,
    decimal NewTotalPrice,
    DateTime ModifiedAt) : IOrderModifiedEvent;

/// <summary>
/// Event indicating order modification failed
/// </summary>
public record OrderModificationFailedEvent : IOrderModifiedEvent
{
    public Guid OrderId { get; }
    public IEnumerable<string> Reasons { get; }

    public OrderModificationFailedEvent(Guid orderId, string reason)
    {
        OrderId = orderId;
        Reasons = new[] { reason };
    }

    public OrderModificationFailedEvent(Guid orderId, IEnumerable<string> reasons)
    {
        OrderId = orderId;
        Reasons = reasons;
    }
}

