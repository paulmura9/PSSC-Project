namespace Ordering.Domain.Models;

/// <summary>
/// Composite Value Object representing contact information
/// </summary>
public sealed record ContactInfo
{
    public PhoneNumber Phone { get; }
    public EmailAddress? Email { get; }

    private ContactInfo(PhoneNumber phone, EmailAddress? email)
    {
        Phone = phone;
        Email = email;
    }

    public static ContactInfo Create(PhoneNumber phone, EmailAddress? email = null)
    {
        return new ContactInfo(phone, email);
    }

    public static ContactInfo Create(string phone, string? email = null)
    {
        return new ContactInfo(
            PhoneNumber.Create(phone),
            email != null ? EmailAddress.Create(email) : null
        );
    }

    public override string ToString() => Email != null 
        ? $"{Phone} / {Email}" 
        : Phone.ToString();
}

