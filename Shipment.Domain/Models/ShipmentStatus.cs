namespace Shipment.Domain.Models;

/// <summary>
/// Value Object representing a shipment status
/// </summary>
public sealed record ShipmentStatus
{
    public string Value { get; }

    private ShipmentStatus(string value)
    {
        Value = value;
    }

    // Predefined statuses
    public static readonly ShipmentStatus Created = new("Created");
    public static readonly ShipmentStatus Validated = new("Validated");
    public static readonly ShipmentStatus Scheduled = new("Scheduled");
    public static readonly ShipmentStatus Dispatched = new("Dispatched");
    public static readonly ShipmentStatus Delivered = new("Delivered");
    public static readonly ShipmentStatus Cancelled = new("Cancelled");
    public static readonly ShipmentStatus Returned = new("Returned");

    public bool CanBeCancelled => 
        this == Created || this == Validated || this == Scheduled;

    public bool CanBeModified => 
        this == Created || this == Validated || this == Scheduled;

    public bool IsTerminal => 
        this == Delivered || this == Cancelled || this == Returned;

    public override string ToString() => Value;
}

