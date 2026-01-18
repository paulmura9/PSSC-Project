namespace Ordering.Domain.Events;

/// <summary>
/// Interface for cancel order workflow result events
/// </summary>
public interface IOrderCancelledEvent { }

/// <summary>
/// Event indicating order was cancelled successfully
/// </summary>
public record OrderCancelledSucceededEvent(
    Guid OrderId,
    Guid UserId,
    string Reason,
    DateTime CancelledAt) : IOrderCancelledEvent;

/// <summary>
/// Event indicating order cancellation failed
/// </summary>
public record OrderCancellationFailedEvent(
    Guid OrderId,
    string FailureReason) : IOrderCancelledEvent;

