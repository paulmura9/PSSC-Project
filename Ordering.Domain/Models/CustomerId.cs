namespace Ordering.Domain.Models;

/// <summary>
/// Value Object representing a customer identifier
/// </summary>
public sealed record CustomerId
{
    public Guid Value { get; }

    private CustomerId(Guid value)
    {
        Value = value;
    }

    public static CustomerId Create(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CustomerId cannot be empty", nameof(value));

        return new CustomerId(value);
    }

    public static CustomerId Create(string value)
    {
        if (!Guid.TryParse(value, out var guid))
            throw new ArgumentException("Invalid CustomerId format", nameof(value));

        return Create(guid);
    }

    public static CustomerId NewId() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
    public static implicit operator Guid(CustomerId id) => id.Value;
}

