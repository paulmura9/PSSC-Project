namespace Ordering.Domain.Models;

/// <summary>
/// Command for cancelling an order
/// </summary>
public record CancelOrderCommand(
    Guid OrderId,
    string Reason);

