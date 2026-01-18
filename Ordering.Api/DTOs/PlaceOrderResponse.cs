using System.ComponentModel.DataAnnotations;

namespace Ordering.Api.DTOs;

/// <summary>
/// Response DTO for a successfully placed order
/// </summary>
public class PlaceOrderResponse
{
    /// <summary>
    /// Unique identifier of the created order
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Whether the customer has a premium subscription (free shipping)
    /// </summary>
    /// <example>false</example>
    public bool PremiumSubscription { get; set; }

    /// <summary>
    /// Subtotal before discount
    /// </summary>
    /// <example>5999.99</example>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// Discount amount applied (from voucher)
    /// </summary>
    /// <example>600.00</example>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// Total price after discount
    /// </summary>
    /// <example>5399.99</example>
    public decimal Total { get; set; }

    /// <summary>
    /// Voucher code used (if any)
    /// </summary>
    /// <example>WELCOME10</example>
    public string? VoucherCode { get; set; }

    /// <summary>
    /// Timestamp when the order was placed
    /// </summary>
    public DateTime OccurredAt { get; set; }

    /// <summary>
    /// List of order lines with calculated prices
    /// </summary>
    public List<OrderLineResponse> Lines { get; set; } = new();
    
    // For backwards compatibility
    public decimal TotalPrice => Total;
}


