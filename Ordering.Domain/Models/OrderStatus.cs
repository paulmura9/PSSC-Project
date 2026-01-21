namespace Ordering.Domain.Models;

/// <summary>
/// Order status values - constants for type safety
/// </summary>
public static class OrderStatus
{
    public const string Placed = "Placed";
    public const string Cancelled = "Cancelled";
    public const string Returned = "Returned";
    public const string Modified = "Modified";
}

