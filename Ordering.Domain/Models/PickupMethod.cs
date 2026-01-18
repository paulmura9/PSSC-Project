namespace Ordering.Domain.Models;

/// <summary>
/// Value Object representing the pickup/delivery method for an order
/// Allowed values: HomeDelivery, EasyBoxPickup, PostOfficePickup
/// </summary>
public sealed record PickupMethod
{
    public string Value { get; }

    public PickupMethod(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Pickup method is required", nameof(value));

        var trimmed = value.Trim();
        
        // Direct case-insensitive comparison
        if (trimmed.Equals("HomeDelivery", StringComparison.OrdinalIgnoreCase))
        {
            Value = "HomeDelivery";
        }
        else if (trimmed.Equals("EasyBoxPickup", StringComparison.OrdinalIgnoreCase))
        {
            Value = "EasyBoxPickup";
        }
        else if (trimmed.Equals("PostOfficePickup", StringComparison.OrdinalIgnoreCase))
        {
            Value = "PostOfficePickup";
        }
        else
        {
            throw new ArgumentException($"Invalid pickup method: {value}. Allowed values: HomeDelivery, EasyBoxPickup, PostOfficePickup", nameof(value));
        }
    }

    /// <summary>
    /// Returns true if this method requires a pickup point ID
    /// </summary>
    public bool RequiresPickupPointId => Value == "EasyBoxPickup" || Value == "PostOfficePickup";

    /// <summary>
    /// Returns true if this method requires a delivery address
    /// </summary>
    public bool RequiresAddress => Value == "HomeDelivery";

    public static bool TryParse(string? value, out PickupMethod? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        try
        {
            result = new PickupMethod(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public override string ToString() => Value;
}
