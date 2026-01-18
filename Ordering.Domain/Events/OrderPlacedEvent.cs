using Ordering.Domain.Models;

namespace Ordering.Domain.Events;

/// <summary>
/// Interface for order placed events
/// </summary>
public interface IOrderPlacedEvent { }

/// <summary>
/// Event indicating order was placed successfully
/// </summary>
public record OrderPlacedEvent : IOrderPlacedEvent
{
    public Guid OrderId { get; }
    public Guid UserId { get; }
    public bool PremiumSubscription { get; }
    public decimal Subtotal { get; }
    public decimal DiscountAmount { get; }
    public decimal Total { get; }
    public string? VoucherCode { get; }
    public IReadOnlyCollection<ValidatedOrderLine> Lines { get; }
    public string Street { get; }
    public string City { get; }
    public string PostalCode { get; }
    public string Phone { get; }
    public string? Email { get; }
    public DateTime OccurredAt { get; }
    
    // For backwards compatibility
    public decimal TotalPrice => Total;

    public OrderPlacedEvent(
        Guid orderId,
        Guid userId,
        bool premiumSubscription,
        decimal subtotal,
        decimal discountAmount,
        decimal total,
        string? voucherCode,
        IReadOnlyCollection<ValidatedOrderLine> lines,
        string street,
        string city,
        string postalCode,
        string phone,
        string? email,
        DateTime occurredAt)
    {
        OrderId = orderId;
        UserId = userId;
        PremiumSubscription = premiumSubscription;
        Subtotal = subtotal;
        DiscountAmount = discountAmount;
        Total = total;
        VoucherCode = voucherCode;
        Lines = lines;
        Street = street;
        City = city;
        PostalCode = postalCode;
        Phone = phone;
        Email = email;
        OccurredAt = occurredAt;
    }
}

/// <summary>
/// Event indicating order placement failed
/// </summary>
public record OrderPlaceFailedEvent : IOrderPlacedEvent
{
    public IEnumerable<string> Reasons { get; }

    public OrderPlaceFailedEvent(string reason)
    {
        Reasons = new[] { reason };
    }

    public OrderPlaceFailedEvent(IEnumerable<string> reasons)
    {
        Reasons = reasons;
    }
}

