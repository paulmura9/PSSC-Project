namespace Shipment.Domain.Models;

/// <summary>
/// Value Object representing a cancellation reason
/// </summary>
public sealed record CancellationReason
{
    public string Value { get; }

    public CancellationReason(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Cancellation reason is required", nameof(value));
        
        if (value.Length > 500)
            throw new ArgumentException("Cancellation reason cannot exceed 500 characters", nameof(value));

        Value = value;
    }

    // Predefined common cancellation reasons
    public static CancellationReason OrderCancelled => new("Order was cancelled by customer");
    public static CancellationReason OutOfStock => new("Product out of stock");
    public static CancellationReason PaymentFailed => new("Payment failed");
    public static CancellationReason Other(string description) => new(description);

    public override string ToString() => Value;
}

