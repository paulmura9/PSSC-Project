namespace Ordering.Domain.Models;

/// <summary>
/// Value Object representing the payment method for an order
/// Allowed values: CashOnDelivery, CardOnDelivery, CardOnline
/// </summary>
public sealed record PaymentMethod
{
    public string Value { get; }

    public PaymentMethod(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Payment method is required", nameof(value));

        var trimmed = value.Trim();
        
        // Direct case-insensitive comparison
        if (trimmed.Equals("CashOnDelivery", StringComparison.OrdinalIgnoreCase))
        {
            Value = "CashOnDelivery";
        }
        else if (trimmed.Equals("CardOnDelivery", StringComparison.OrdinalIgnoreCase))
        {
            Value = "CardOnDelivery";
        }
        else if (trimmed.Equals("CardOnline", StringComparison.OrdinalIgnoreCase))
        {
            Value = "CardOnline";
        }
        else
        {
            throw new ArgumentException($"Invalid payment method: {value}. Allowed values: CashOnDelivery, CardOnDelivery, CardOnline", nameof(value));
        }
    }

    public static bool TryParse(string? value, out PaymentMethod? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        try
        {
            result = new PaymentMethod(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public override string ToString() => Value;
}
