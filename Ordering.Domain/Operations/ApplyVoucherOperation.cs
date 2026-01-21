using Microsoft.Extensions.Logging;
using Ordering.Domain.Models;
using static Ordering.Domain.Models.Order;

namespace Ordering.Domain.Operations;

/// <summary>
/// Operation that applies a voucher discount to a validated order
/// Transforms ValidatedOrder to PricedOrder with discount applied
/// </summary>
public class ApplyVoucherOperation
{
    private readonly IVoucherRepository _voucherRepository;
    private readonly ILogger<ApplyVoucherOperation> _logger;

    public ApplyVoucherOperation(
        IVoucherRepository voucherRepository,
        ILogger<ApplyVoucherOperation> logger)
    {
        _voucherRepository = voucherRepository;
        _logger = logger;
    }

    /// <summary>
    /// Applies voucher discount to order
    /// </summary>
    /// <param name="order">Validated order</param>
    /// <param name="subtotal">Calculated subtotal from lines</param>
    /// <param name="voucherCode">Optional voucher code</param>
    /// <returns>PricedOrder with discount applied or InvalidOrder if voucher is invalid</returns>
    public async Task<IOrder> ExecuteAsync(
        ValidatedOrder order,
        decimal subtotal,
        string? voucherCode,
        CancellationToken cancellationToken = default)
    {
        // If no voucher code provided, return order without discount
        if (string.IsNullOrWhiteSpace(voucherCode))
        {
            _logger.LogInformation("No voucher code provided, no discount applied");
            return CreatePricedOrder(order, subtotal, 0, subtotal, null);
        }

        // Validate and normalize voucher code using VO
        var voucherCodeVo = VoucherCode.TryCreate(voucherCode);
        if (voucherCodeVo == null)
        {
            _logger.LogWarning("Invalid voucher code format: {VoucherCode}", voucherCode);
            return new InvalidOrder(
                order.Lines.Select(l => UnvalidatedOrderLine.Create(
                    l.Name.Value, l.Description.Value, l.Category.Value,
                    l.Quantity.Value, l.UnitPrice.Value)).ToList().AsReadOnly(),
                new[] { "Invalid voucher code format" });
        }

        var normalizedCode = voucherCodeVo.Value;
        _logger.LogInformation("Applying voucher code: {VoucherCode}", normalizedCode);

        // Get voucher from repository
        var voucher = await _voucherRepository.GetActiveByCodeAsync(normalizedCode, cancellationToken);

        if (voucher == null)
        {
            _logger.LogWarning("Voucher not found: {VoucherCode}", normalizedCode);
            return new InvalidOrder(
                order.Lines.Select(l => UnvalidatedOrderLine.Create(
                    l.Name.Value, l.Description.Value, l.Category.Value, 
                    l.Quantity.Value, l.UnitPrice.Value)).ToList().AsReadOnly(),
                new[] { $"Voucher '{normalizedCode}' not found" });
        }

        // Validate voucher
        var utcNow = DateTime.UtcNow;
        var validationError = voucher.GetValidationError(utcNow);
        
        if (validationError != null)
        {
            _logger.LogWarning("Voucher validation failed: {Error}", validationError);
            return new InvalidOrder(
                order.Lines.Select(l => UnvalidatedOrderLine.Create(
                    l.Name.Value, l.Description.Value, l.Category.Value,
                    l.Quantity.Value, l.UnitPrice.Value)).ToList().AsReadOnly(),
                new[] { validationError });
        }

        // Try to consume the voucher (increment uses atomically)
        var consumed = await _voucherRepository.TryConsumeAsync(voucher.VoucherId, cancellationToken);
        
        if (!consumed)
        {
            _logger.LogWarning("Voucher exhausted: {VoucherCode}", normalizedCode);
            return new InvalidOrder(
                order.Lines.Select(l => UnvalidatedOrderLine.Create(
                    l.Name.Value, l.Description.Value, l.Category.Value,
                    l.Quantity.Value, l.UnitPrice.Value)).ToList().AsReadOnly(),
                new[] { "Voucher has been exhausted" });
        }

        // Calculate discount
        var discountPercent = DiscountPercent.Create(voucher.DiscountPercent);
        var discountAmount = discountPercent.CalculateDiscount(subtotal);
        var total = Math.Max(0, subtotal - discountAmount);

        _logger.LogInformation(
            "Voucher applied successfully. Subtotal: {Subtotal}, Discount: {Discount}% ({DiscountAmount}), Total: {Total}",
            subtotal, voucher.DiscountPercent, discountAmount, total);

        return CreatePricedOrder(order, subtotal, discountAmount, total, normalizedCode);
    }

    private static PricedOrder CreatePricedOrder(
        ValidatedOrder order,
        decimal subtotal,
        decimal discountAmount,
        decimal total,
        string? voucherCode)
    {
        var voucherCodeVo = !string.IsNullOrWhiteSpace(voucherCode) 
            ? VoucherCode.Create(voucherCode) 
            : null;
            
        return new PricedOrder(
            order.Lines,
            order.UserId,
            order.Street,
            order.City,
            order.PostalCode,
            order.Phone,
            order.Email,
            order.DeliveryNotes,
            subtotal,
            discountAmount,
            total,
            voucherCodeVo,
            order.PremiumSubscription,
            order.PickupMethod,
            order.PickupPointId,
            order.PaymentMethod);
    }
}

