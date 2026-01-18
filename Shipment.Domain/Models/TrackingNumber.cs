namespace Shipment.Domain.Models;

/// <summary>
/// Value Object representing a tracking number
/// </summary>
public sealed record TrackingNumber
{
    public string Value { get; }

    public TrackingNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Tracking number is required", nameof(value));
        
        if (value.Length > 100)
            throw new ArgumentException("Tracking number cannot exceed 100 characters", nameof(value));

        Value = value;
    }

    public static TrackingNumber Generate()
    {
        var trackingNumber = $"TRACK-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
        return new TrackingNumber(trackingNumber);
    }

    public override string ToString() => Value;
}

