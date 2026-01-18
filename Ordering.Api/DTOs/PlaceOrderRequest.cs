using System.ComponentModel.DataAnnotations;
using Ordering.Domain.Models;

namespace Ordering.Api.DTOs;

/// <summary>
/// Request DTO for placing an order
/// </summary>
public class PlaceOrderRequest
{
    /// <summary>
    /// Unique identifier for the user placing the order
    /// </summary>
    [Required(ErrorMessage = "UserId is required")]
    public Guid UserId { get; set; }

    /// <summary>
    /// Street address (required for HomeDelivery, optional for pickup methods)
    /// </summary>
    [StringLength(200, MinimumLength = 5, ErrorMessage = "Street must be between 5 and 200 characters")]
    public string? Street { get; set; }

    /// <summary>
    /// City name (required for HomeDelivery, optional for pickup methods)
    /// </summary>
    [StringLength(100, MinimumLength = 2, ErrorMessage = "City must be between 2 and 100 characters")]
    public string? City { get; set; }

    /// <summary>
    /// Postal code (required for HomeDelivery, optional for pickup methods)
    /// </summary>
    [RegularExpression(@"^\d{5,6}$", ErrorMessage = "Postal code must be 5-6 digits")]
    public string? PostalCode { get; set; }

    /// <summary>
    /// Phone number in Romanian format (10 digits starting with 07)
    /// </summary>
    [Required(ErrorMessage = "Phone number is required")]
    [RegularExpression(@"^07\d{8}$", ErrorMessage = "Phone must be a valid Romanian mobile number (07xxxxxxxx)")]
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Email address (optional)
    /// </summary>
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(254, ErrorMessage = "Email cannot exceed 254 characters")]
    public string? Email { get; set; }

    /// <summary>
    /// Optional delivery notes
    /// </summary>
    [StringLength(250, ErrorMessage = "Delivery notes cannot exceed 250 characters")]
    public string? DeliveryNotes { get; set; }

    /// <summary>
    /// List of products to order
    /// </summary>
    [Required(ErrorMessage = "At least one product is required")]
    [MinLength(1, ErrorMessage = "At least one product is required")]
    public List<ProductLineInput> Products { get; set; } = new();

    /// <summary>
    /// Optional voucher code for discount
    /// </summary>
    /// <example>WELCOME10</example>
    [StringLength(64, ErrorMessage = "Voucher code cannot exceed 64 characters")]
    public string? VoucherCode { get; set; }

    /// <summary>
    /// Whether the customer has a premium subscription (free shipping)
    /// </summary>
    /// <example>false</example>
    public bool PremiumSubscription { get; set; } = false;

    /// <summary>
    /// Pickup/Delivery method: HomeDelivery, EasyBoxPickup, PostOfficePickup
    /// </summary>
    /// <example>HomeDelivery</example>
    [Required(ErrorMessage = "Pickup method is required")]
    [RegularExpression(@"^(HomeDelivery|EasyBoxPickup|PostOfficePickup)$", 
        ErrorMessage = "Pickup method must be HomeDelivery, EasyBoxPickup, or PostOfficePickup")]
    public string PickupMethod { get; set; } = "HomeDelivery";

    /// <summary>
    /// Pickup point ID (required for EasyBoxPickup and PostOfficePickup, must be null for HomeDelivery)
    /// </summary>
    /// <example>EBX-12345</example>
    [StringLength(64, ErrorMessage = "Pickup point ID cannot exceed 64 characters")]
    public string? PickupPointId { get; set; }

    /// <summary>
    /// Payment method: CashOnDelivery, CardOnDelivery, CardOnline
    /// </summary>
    /// <example>CashOnDelivery</example>
    [Required(ErrorMessage = "Payment method is required")]
    [RegularExpression(@"^(CashOnDelivery|CardOnDelivery|CardOnline)$", 
        ErrorMessage = "Payment method must be CashOnDelivery, CardOnDelivery, or CardOnline")]
    public string PaymentMethod { get; set; } = "CashOnDelivery";

    /// <summary>
    /// Converts all fields to domain Value Objects (composite structure)
    /// </summary>
    public PlaceOrderDomainData ToDomain()
    {
        return new PlaceOrderDomainData(
            CustomerId.Create(UserId),
            Street != null && City != null && PostalCode != null 
                ? DeliveryAddress.Create(Street, City, PostalCode) 
                : null,
            ContactInfo.Create(Phone, Email),
            DeliveryNotes != null ? Ordering.Domain.Models.DeliveryNotes.Create(DeliveryNotes) : null
        );
    }
}

/// <summary>
/// Domain data with composite Value Objects for order placement
/// </summary>
public record PlaceOrderDomainData(
    CustomerId CustomerId,
    DeliveryAddress? DeliveryAddress,
    ContactInfo ContactInfo,
    DeliveryNotes? DeliveryNotes
);

