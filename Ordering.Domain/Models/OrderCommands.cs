namespace Ordering.Domain.Models;

/// <summary>
/// Command for cancelling an order
/// </summary>
public record CancelOrderCommand(
    Guid OrderId,
    string Reason);

/// <summary>
/// Command for modifying an order
/// </summary>
public record ModifyOrderCommand(
    Guid OrderId,
    IReadOnlyCollection<UnvalidatedOrderLine> Lines,
    string Street,
    string City,
    string PostalCode,
    string Phone,
    string? Email,
    string? DeliveryNotes);

/// <summary>
/// Command for returning an order
/// </summary>
public record ReturnOrderCommand(
    Guid OrderId,
    string ReturnReason);

