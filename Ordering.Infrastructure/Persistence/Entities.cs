namespace Ordering.Infrastructure.Persistence;

/// <summary>
/// Entity representing an order in the database
/// </summary>
public class OrderEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string CardNumberMasked { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<OrderLineEntity> Lines { get; set; } = new List<OrderLineEntity>();
}

/// <summary>
/// Entity representing an order line in the database
/// </summary>
public class OrderLineEntity
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }

    public OrderEntity Order { get; set; } = null!;
}

