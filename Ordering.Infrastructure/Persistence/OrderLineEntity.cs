namespace Ordering.Infrastructure.Persistence;

/// <summary>
/// Entity representing an order line in the database
/// </summary>
public class OrderLineEntity
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }

    public OrderEntity Order { get; set; } = null!;
}

