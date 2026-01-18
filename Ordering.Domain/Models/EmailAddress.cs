namespace Ordering.Domain.Models;

/// <summary>
/// Value Object representing an email address (optional)
/// </summary>
public sealed record EmailAddress
{
    public string Value { get; }

    private EmailAddress(string value)
    {
        Value = value;
    }

    public static EmailAddress Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email is required", nameof(value));

        if (!value.Contains('@') || !value.Contains('.'))
            throw new ArgumentException("Invalid email format", nameof(value));

        if (value.Length > 254)
            throw new ArgumentException("Email cannot exceed 254 characters", nameof(value));

        return new EmailAddress(value.Trim().ToLowerInvariant());
    }

    public override string ToString() => Value;
    public static implicit operator string(EmailAddress email) => email.Value;
}

