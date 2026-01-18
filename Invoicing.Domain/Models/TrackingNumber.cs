namespace Invoicing.Models;

/// <summary>
/// Value Object representing tracking number in invoicing
/// Data comes pre-validated from Shipment
/// </summary>
public sealed record TrackingNumber
{
    public string Value { get; }

    public TrackingNumber(string value)
    {
        // Data is pre-validated from Shipment
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public override string ToString() => Value;
}

