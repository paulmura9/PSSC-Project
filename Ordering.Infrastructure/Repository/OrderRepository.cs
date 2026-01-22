using Microsoft.EntityFrameworkCore;
using Ordering.Domain.Models;
using Ordering.Infrastructure.Persistence;
using SharedKernel.Ordering;
using static Ordering.Domain.Models.Order;
using DomainOrderRepository = Ordering.Domain.Repositories.IOrderRepository;

namespace Ordering.Infrastructure.Repository;

/// <summary>
/// EF Core implementation of IOrderRepository
/// </summary>
public class OrderRepository : DomainOrderRepository
{
    private readonly OrderingDbContext _context;

    public OrderRepository(OrderingDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> SaveOrderAsync(OrderSaveData order, CancellationToken cancellationToken = default)
    {
        var orderEntity = new OrderEntity
        {
            Id = order.OrderId,
            UserId = order.UserId,
            Street = order.Street,
            City = order.City,
            PostalCode = order.PostalCode,
            Phone = order.Phone,
            Email = order.Email,
            DeliveryNotes = order.DeliveryNotes,
            Subtotal = order.Subtotal,
            DiscountAmount = order.DiscountAmount,
            Total = order.Total,
            VoucherCode = order.VoucherCode,
            PremiumSubscription = order.PremiumSubscription,
            PickupMethod = order.PickupMethod,
            PickupPointId = order.PickupPointId,
            PaymentMethod = order.PaymentMethod,
            CreatedAt = order.CreatedAt,
            Lines = order.Lines.Select(line => new OrderLineEntity
            {
                Id = line.OrderLineId,
                OrderId = order.OrderId,
                Name = line.Name,
                Description = line.Description,
                Category = line.Category,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                LineTotal = line.LineTotal
            }).ToList()
        };

        await _context.Orders.AddAsync(orderEntity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return order.OrderId;
    }

    public async Task<OrderQueryResult?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (entity == null)
            return null;

        return new OrderQueryResult
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Street = entity.Street,
            City = entity.City,
            PostalCode = entity.PostalCode,
            Phone = entity.Phone,
            Email = entity.Email,
            Subtotal = entity.Subtotal,
            DiscountAmount = entity.DiscountAmount,
            Total = entity.Total,
            VoucherCode = entity.VoucherCode,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            PickupMethod = entity.PickupMethod,
            PickupPointId = entity.PickupPointId,
            PaymentMethod = entity.PaymentMethod
        };
    }

    public async Task UpdateStatusAsync(Guid orderId, string newStatus, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Orders.FindAsync(new object[] { orderId }, cancellationToken);
        if (entity != null)
        {
            entity.Status = newStatus;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UpdateOrderAsync(
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
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.Orders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (entity != null)
        {
            entity.Street = street;
            entity.City = city;
            entity.PostalCode = postalCode;
            entity.Phone = phone;
            entity.Email = email;
            entity.Subtotal = subtotal;
            entity.DiscountAmount = discountAmount;
            entity.Total = total;
            entity.VoucherCode = voucherCode;

            // Remove old lines and add new ones
            _context.OrderLines.RemoveRange(entity.Lines);

            foreach (var line in lines)
            {
                entity.Lines.Add(new OrderLineEntity
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    Name = line.Name.Value,
                    Description = line.Description.Value,
                    Category = line.Category.Value,
                    Quantity = line.Quantity.Value,
                    UnitPrice = line.UnitPrice.Value,
                    LineTotal = line.LineTotal.Value
                });
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

