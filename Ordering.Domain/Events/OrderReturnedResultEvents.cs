namespace Ordering.Domain.Events;

/// <summary>
/// Interface for return order workflow result events
/// </summary>
public interface IOrderReturnedEvent { }

/// <summary>
/// Event indicating order was returned successfully
/// </summary>
public record OrderReturnSucceededEvent(
    Guid OrderId,
    Guid UserId,
    string ReturnReason,
    DateTime ReturnedAt) : IOrderReturnedEvent;

/// <summary>
/// Event indicating order return failed
/// </summary>
public record OrderReturnFailedEvent(
    Guid OrderId,
    string FailureReason) : IOrderReturnedEvent;

