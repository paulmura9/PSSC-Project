using System.ComponentModel.DataAnnotations;

namespace Ordering.Api.DTOs;

/// <summary>
/// Request to cancel an order
/// </summary>
public class CancelOrderRequest
{
    /// <summary>
    /// Reason for cancellation
    /// </summary>
    [Required(ErrorMessage = "Cancellation reason is required")]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Reason must be between 5 and 500 characters")]
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Response after order cancellation
/// </summary>
public class CancelOrderResponse
{
    public Guid OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime CancelledAt { get; set; }
}

/// <summary>
/// Request to return an order
/// </summary>
public class ReturnOrderRequest
{
    /// <summary>
    /// Reason for return
    /// </summary>
    [Required(ErrorMessage = "Return reason is required")]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Reason must be between 5 and 500 characters")]
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Response after order return
/// </summary>
public class ReturnOrderResponse
{
    public Guid OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime ReturnedAt { get; set; }
}

