using Ordering.Domain.Models;
using static Ordering.Domain.Models.Order;

namespace Ordering.Domain.Workflows;

/// <summary>
/// Command to place an order with optional voucher code
/// </summary>
public sealed record PlaceOrderCommand
{
    public UnvalidatedOrder UnvalidatedOrder { get; }
    public string? VoucherCode { get; }

    public PlaceOrderCommand(UnvalidatedOrder unvalidatedOrder, string? voucherCode = null)
    {
        UnvalidatedOrder = unvalidatedOrder;
        VoucherCode = voucherCode;
    }
}

