namespace Ordering.Api.DTOs;

/// <summary>
/// Response DTO for a failed order placement
/// </summary>
public class PlaceOrderErrorResponse
{
    /// <summary>
    /// List of validation or business errors
    /// </summary>
    /// <example>["Product 'Laptop' not found", "Insufficient stock"]</example>
    public List<string> Errors { get; set; } = new();
}

