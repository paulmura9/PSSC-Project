using Ordering.Domain.Models;
using static Ordering.Domain.Models.Order;

namespace Ordering.Domain.Workflows;

/// <summary>
/// Command to place an order
/// </summary>
public sealed record PlaceOrderCommand
{
    public Order.UnvalidatedOrder UnvalidatedOrder { get; }

    public PlaceOrderCommand(Order.UnvalidatedOrder unvalidatedOrder)
    {
        UnvalidatedOrder = unvalidatedOrder;
    }
}

