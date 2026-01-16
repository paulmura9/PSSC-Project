using System.ComponentModel.DataAnnotations;

namespace Ordering.Api.DTOs;

/// <summary>
/// Request DTO for placing an order
/// </summary>
public class PlaceOrderRequest
{
    public Guid UserId { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string CardNumber { get; set; } = string.Empty;
    public string Cvv { get; set; } = string.Empty;
    public string Expiry { get; set; } = string.Empty;
    public List<ProductLineInput> Products { get; set; } = new();
}

/// <summary>
/// Product line input DTO
/// </summary>
public class ProductLineInput
{
    
    public string Name { get; set; } = string.Empty;
    [Range(0,100)]
    public int Quantity { get; set; }
    

    public decimal UnitPrice { get; set; }
}

