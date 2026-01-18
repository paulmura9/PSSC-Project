namespace Ordering.Domain.Models;

/// <summary>
/// Composite Value Object representing a full delivery address
/// </summary>
public sealed record DeliveryAddress
{
    public Street Street { get; }
    public City City { get; }
    public PostalCode PostalCode { get; }

    private DeliveryAddress(Street street, City city, PostalCode postalCode)
    {
        Street = street;
        City = city;
        PostalCode = postalCode;
    }

    public static DeliveryAddress Create(Street street, City city, PostalCode postalCode)
    {
        return new DeliveryAddress(street, city, postalCode);
    }

    public static DeliveryAddress Create(string street, string city, string postalCode)
    {
        return new DeliveryAddress(
            Street.Create(street),
            City.Create(city),
            PostalCode.Create(postalCode)
        );
    }

    /// <summary>
    /// Creates DeliveryAddress from a single string (parses "Street, City, PostalCode")
    /// </summary>
    public static DeliveryAddress CreateFromString(string fullAddress)
    {
        if (string.IsNullOrWhiteSpace(fullAddress))
            throw new ArgumentException("Delivery address cannot be empty", nameof(fullAddress));

        var parts = fullAddress.Split(',', StringSplitOptions.TrimEntries);
        
        if (parts.Length >= 3)
        {
            return new DeliveryAddress(
                Street.Create(parts[0]),
                City.Create(parts[1]),
                PostalCode.Create(parts[2])
            );
        }
        
        // Fallback for simple addresses
        return new DeliveryAddress(
            Street.Create(fullAddress.Length >= 5 ? fullAddress : fullAddress + " - "),
            City.Create("Unknown"),
            PostalCode.Create("000000")
        );
    }

    public string FullAddress => $"{Street}, {City}, {PostalCode}";
    public override string ToString() => FullAddress;
    public static implicit operator string(DeliveryAddress address) => address.FullAddress;
}

