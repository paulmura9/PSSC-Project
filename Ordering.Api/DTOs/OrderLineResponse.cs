using System.ComponentModel.DataAnnotations;

namespace Ordering.Api.DTOs;

/// <summary>
/// Order line response DTO with calculated totals
/// </summary>
public class OrderLineResponse
{
    /// <summary>
    /// Product name
    /// </summary>
    /// <example>Laptop</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Product description
    /// </summary>
    /// <example>Gaming laptop with RTX 4070</example>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Product category
    /// </summary>
    /// <example>Electronics</example>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Quantity ordered
    /// </summary>
    /// <example>2</example>
    [Range(1, 100)]
    public int Quantity { get; set; }

    /// <summary>
    /// Unit price from catalog
    /// </summary>
    /// <example>2749.99</example>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Line total (Quantity × UnitPrice)
    /// </summary>
    /// <example>5499.98</example>
    public decimal LineTotal { get; set; }
}

