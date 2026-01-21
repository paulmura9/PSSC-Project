namespace Ordering.Domain.Models;

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

