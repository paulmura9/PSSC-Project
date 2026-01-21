using Ordering.Domain.Models;
using static Ordering.Domain.Models.Order;

namespace Ordering.Domain.Operations;

/// <summary>
/// Interface for order repository operations
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// Saves a new order to the database and returns the generated order ID
    /// </summary>
    Task<Guid> SaveOrderAsync(PersistableOrder order, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an order by its ID
    /// </summary>
    Task<OrderQueryResult?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the status of an order
    /// </summary>
    Task UpdateStatusAsync(Guid orderId, string newStatus, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an order with new data (for modify workflow)
    /// </summary>
    Task UpdateOrderAsync(
        Guid orderId,
        string street,
        string city,
        string postalCode,
        string phone,
        string? email,
        decimal subtotal,
        decimal discountAmount,
        decimal total,
        string? voucherCode,
        IReadOnlyCollection<ValidatedOrderLine> lines,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// DTO for order data returned from repository
/// Used to avoid dependency on Infrastructure entities in Domain
/// </summary>
public record OrderQueryResult
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string? Street { get; init; }
    public string? City { get; init; }
    public string? PostalCode { get; init; }
    public string Phone { get; init; } = string.Empty;
    public string? Email { get; init; }
    public decimal Subtotal { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal Total { get; init; }
    public string? VoucherCode { get; init; }
    public string Status { get; init; } = "Sent";
    public DateTime CreatedAt { get; init; }
    
    // Pickup/Delivery method
    public string PickupMethod { get; init; } = "HomeDelivery";
    public string? PickupPointId { get; init; }
    
    // Payment fields
    public string PaymentMethod { get; init; } = "CashOnDelivery";
    
    // For backwards compatibility
    public decimal TotalPrice => Total;
}

