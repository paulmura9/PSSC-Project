namespace Ordering.Domain.Operations;

/// <summary>
/// Interface for order persistence operations
/// </summary>
public interface IPersistence
{
    /// <summary>
    /// Saves an order to the database and returns the generated order ID
    /// </summary>
    Task<Guid> SaveOrderAsync(PersistableOrder order, CancellationToken cancellationToken);
}

/// <summary>
/// DTO for order data to be persisted
/// </summary>
public sealed record PersistableOrder
{
    public Guid UserId { get; }
    public string DeliveryAddress { get; }
    public string PostalCode { get; }
    public string Phone { get; }
    public string CardNumberMasked { get; }
    public decimal TotalPrice { get; }
    public IReadOnlyCollection<PersistableOrderLine> Lines { get; }

    public PersistableOrder(
        Guid userId,
        string deliveryAddress,
        string postalCode,
        string phone,
        string cardNumberMasked,
        decimal totalPrice,
        IReadOnlyCollection<PersistableOrderLine> lines)
    {
        UserId = userId;
        DeliveryAddress = deliveryAddress;
        PostalCode = postalCode;
        Phone = phone;
        CardNumberMasked = cardNumberMasked;
        TotalPrice = totalPrice;
        Lines = lines;
    }
}

/// <summary>
/// DTO for order line data to be persisted
/// </summary>
public sealed record PersistableOrderLine
{
    public string Name { get; }
    public int Quantity { get; }
    public decimal UnitPrice { get; }
    public decimal LineTotal { get; }

    public PersistableOrderLine(string name, int quantity, decimal unitPrice, decimal lineTotal)
    {
        Name = name;
        Quantity = quantity;
        UnitPrice = unitPrice;
        LineTotal = lineTotal;
    }
}

