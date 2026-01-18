namespace Shipment.Domain.Models;

/// <summary>
/// Value Object representing a return reason
/// </summary>
public sealed record ReturnReason
{
    public string Value { get; }

    public ReturnReason(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Return reason is required", nameof(value));
        
        if (value.Length > 500)
            throw new ArgumentException("Return reason cannot exceed 500 characters", nameof(value));

        Value = value;
    }

    // Predefined common return reasons
    public static ReturnReason Defective => new("Product is defective");
    public static ReturnReason WrongItem => new("Wrong item received");
    public static ReturnReason NotAsDescribed => new("Product not as described");
    public static ReturnReason ChangedMind => new("Customer changed mind");
    public static ReturnReason Other(string description) => new(description);

    public override string ToString() => Value;
}

