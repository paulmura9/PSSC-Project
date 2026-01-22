namespace SharedKernel.Ordering;

/// <summary>
/// Interface for order repository operations
/// Port in SharedKernel, implemented in Infrastructure
/// </summary>
public interface IOrderRepository
{
    Task<Guid> SaveOrderAsync(OrderSaveData order, CancellationToken cancellationToken = default);
    Task<OrderQueryResult?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(Guid orderId, string newStatus, CancellationToken cancellationToken = default);
}

/// <summary>
/// DTO for saving order data to repository
/// </summary>
public record OrderSaveData
{
    public Guid OrderId { get; init; }
    public Guid UserId { get; init; }
    public string? Street { get; init; }
    public string? City { get; init; }
    public string? PostalCode { get; init; }
    public string Phone { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? DeliveryNotes { get; init; }
    public decimal Subtotal { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal Total { get; init; }
    public string? VoucherCode { get; init; }
    public bool PremiumSubscription { get; init; }
    public string PickupMethod { get; init; } = string.Empty;
    public string? PickupPointId { get; init; }
    public string PaymentMethod { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public IReadOnlyCollection<OrderLineSaveData> Lines { get; init; } = Array.Empty<OrderLineSaveData>();
}

/// <summary>
/// DTO for saving order line data
/// </summary>
public record OrderLineSaveData
{
    public Guid OrderLineId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Category { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal LineTotal { get; init; }
}

/// <summary>
/// DTO for order data returned from repository
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
    public string? DeliveryNotes { get; init; }
    public decimal Subtotal { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal Total { get; init; }
    public string? VoucherCode { get; init; }
    public bool PremiumSubscription { get; init; }
    public string PickupMethod { get; init; } = "HomeDelivery";
    public string? PickupPointId { get; init; }
    public string PaymentMethod { get; init; } = "CashOnDelivery";
    public string Status { get; init; } = "Sent";
    public DateTime CreatedAt { get; init; }
    

}

