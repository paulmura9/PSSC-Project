using Ordering.Domain.Operations;

namespace Ordering.Infrastructure.Persistence;

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

        var orderEntity = new OrderEntity
        {
            Id = orderId,
            UserId = order.UserId,
            DeliveryAddress = order.DeliveryAddress,
            PostalCode = order.PostalCode,
            Phone = order.Phone,
            CardNumberMasked = order.CardNumberMasked,
            TotalPrice = order.TotalPrice,
            CreatedAt = createdAt,
            Lines = order.Lines.Select(line => new OrderLineEntity
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                Name = line.Name,
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

