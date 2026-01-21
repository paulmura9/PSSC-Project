namespace Ordering.Domain.Models;

/// <summary>
/// Command for returning an order
/// </summary>
public record ReturnOrderCommand(
    Guid OrderId,
    string ReturnReason);

