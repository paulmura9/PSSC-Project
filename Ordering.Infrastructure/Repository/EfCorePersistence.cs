using Ordering.Domain.Operations;
using Ordering.Infrastructure.Persistence;
using static Ordering.Domain.Models.Order;

namespace Ordering.Infrastructure.Repository;

/// <summary>
/// EF Core implementation of the persistence interface
/// </summary>
public class EfCorePersistence : IPersistence
{
    private readonly OrderingDbContext _dbContext;

    public EfCorePersistence(OrderingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> SaveOrderAsync(PersistableOrder order, CancellationToken cancellationToken)
    {
        var orderId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        var orderEntity = new Persistence.OrderEntity
        {
            Id = orderId,
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
            PickupMethod = order.PickupMethod,
            PickupPointId = order.PickupPointId,
            PaymentMethod = order.PaymentMethod,
            CreatedAt = createdAt,
            Lines = order.Lines.Select(line => new Persistence.OrderLineEntity
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                Name = line.Name,
                Description = line.Description,
                Category = line.Category,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                LineTotal = line.LineTotal
            }).ToList()
        };

        await _dbContext.Orders.AddAsync(orderEntity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return orderId;
    }
}

