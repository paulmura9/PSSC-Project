namespace Ordering.Api.DTOs;

/// <summary>
/// Response DTO for a successfully placed order
/// </summary>
public class PlaceOrderResponse
{
    public Guid OrderId { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime OccurredAt { get; set; }
    public List<OrderLineResponse> Lines { get; set; } = new();
}

/// <summary>
/// Order line response DTO
/// </summary>
public class OrderLineResponse
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

/// <summary>
/// Response DTO for a failed order placement
/// </summary>
public class PlaceOrderErrorResponse
{
    public List<string> Errors { get; set; } = new();
}

