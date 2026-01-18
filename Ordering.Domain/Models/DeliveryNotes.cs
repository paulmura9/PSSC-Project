namespace Ordering.Domain.Models;

/// <summary>
/// Value Object representing optional delivery notes
/// </summary>
public sealed record DeliveryNotes
{
    public string Value { get; }

    private DeliveryNotes(string value)
    {
        Value = value;
    }

    public static DeliveryNotes Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new DeliveryNotes(string.Empty);

        if (value.Length > 250)
            throw new ArgumentException("Delivery notes cannot exceed 250 characters", nameof(value));

        return new DeliveryNotes(value.Trim());
    }

    public static DeliveryNotes Empty => new(string.Empty);

    public bool HasValue => !string.IsNullOrEmpty(Value);

    public override string ToString() => Value;
    public static implicit operator string(DeliveryNotes notes) => notes.Value;
}

