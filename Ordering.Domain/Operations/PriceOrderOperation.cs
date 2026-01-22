using Microsoft.Extensions.Logging;
using Ordering.Domain.Models;
using static Ordering.Domain.Models.Order;

namespace Ordering.Domain.Operations;

/// <summary>
/// Operation that calculates the total price for a validated order
/// Includes voucher discount application
/// </summary>
public class PriceOrderOperation
{
    private readonly ApplyVoucherOperation _applyVoucherOperation;
    private readonly ILogger<PriceOrderOperation> _logger;

    public PriceOrderOperation(
        ApplyVoucherOperation applyVoucherOperation,
        ILogger<PriceOrderOperation> logger)
    {
        _applyVoucherOperation = applyVoucherOperation;
        _logger = logger;
    }

    /// <summary>
    /// Calculates order price with optional voucher discount
    /// </summary>
    public async Task<IOrder> ExecuteAsync(
        ValidatedOrder order,
        string? voucherCode,
        CancellationToken cancellationToken = default)
    {
        // Calculate subtotal from lines (suma la toate produsele)
        var subtotal = order.Lines.Sum(line => line.LineTotal.Value);
        _logger.LogInformation("Calculated subtotal: {Subtotal}", subtotal);

        // Apply voucher if provided
        var result = await _applyVoucherOperation.ExecuteAsync(
            order, 
            subtotal, 
            voucherCode, 
            cancellationToken);

        return result;
    }
}

