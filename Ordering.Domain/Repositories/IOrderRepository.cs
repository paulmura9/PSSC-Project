﻿using Ordering.Domain.Models;
using SharedKernel.Ordering;
using static Ordering.Domain.Models.Order;

namespace Ordering.Domain.Repositories;

/// <summary>
/// Interface for order repository operations
/// Extends the base interface from SharedKernel
/// </summary>
public interface IOrderRepository : SharedKernel.Ordering.IOrderRepository
{
    /// <summary>
    /// Updates an order with new data (for modify workflow)
    /// </summary>
    Task UpdateOrderAsync(
        Guid orderId,
        string street,
        string city,
        string postalCode,
        string phone,
        string? email,
        decimal subtotal,
        decimal discountAmount,
        decimal total,
        string? voucherCode,
        IReadOnlyCollection<ValidatedOrderLine> lines,
        CancellationToken cancellationToken = default);
}

