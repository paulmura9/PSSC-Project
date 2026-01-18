using System.ComponentModel.DataAnnotations;
using Ordering.Domain.Models;

namespace Ordering.Api.DTOs;

/// <summary>
/// Product line input DTO for ordering
/// Only needs product name and quantity - Description, Category, and Price are fetched from the Products catalog
/// </summary>
public class ProductLineInput
{
    /// <summary>
    /// Product name as it appears in the catalog
    /// </summary>
    /// <example>Laptop</example>
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 200 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Quantity to order (1-10000)
    /// </summary>
    /// <example>1</example>
    [Required(ErrorMessage = "Quantity is required")]
    [Range(1, 10000, ErrorMessage = "Quantity must be between 1 and 10,000")]
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// Converts to domain Value Objects
    /// </summary>
    public (ProductName Name, Ordering.Domain.Models.Quantity Quantity) ToDomain()
    {
        return (ProductName.Create(Name), Ordering.Domain.Models.Quantity.Create(Quantity));
    }
}

