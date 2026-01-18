namespace Ordering.Domain.Models;

/// <summary>
/// Value Object representing a pickup point identifier
/// Required for EasyBoxPickup and PostOfficePickup methods
/// </summary>
public sealed record PickupPointId
{
    public const int MaxLength = 64;

    public string Value { get; }

    public PickupPointId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Pickup point ID is required", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"Pickup point ID cannot exceed {MaxLength} characters", nameof(value));

        Value = trimmed;
    }

    public static bool TryParse(string? value, out PickupPointId? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        try
        {
            result = new PickupPointId(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public override string ToString() => Value;
}

