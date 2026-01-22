using Microsoft.EntityFrameworkCore;
using SharedKernel.Shipment;
using Shipment.Infrastructure.Persistence;

namespace Shipment.Infrastructure.Repository;

/// <summary>
/// Repository implementation for Shipment persistence operations
/// Shipment reacts to events from Ordering
/// Maps EF entities to domain models (like lab pattern)
/// </summary>
public class ShipmentRepository : IShipmentRepository
{
    private readonly ShipmentDbContext _context;

    public ShipmentRepository(ShipmentDbContext context)
    {
        _context = context;
    }

    public async Task SaveShipmentAsync(ShipmentSaveData shipment, CancellationToken cancellationToken = default)
    {
        // Check if shipment already exists (idempotency check for Service Bus retries)
        var existingShipment = await _context.Shipments
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ShipmentId == shipment.ShipmentId || s.OrderId == shipment.OrderId, cancellationToken);
        
        if (existingShipment != null)
        {
            // Shipment already exists, skip (idempotent operation)
            return;
        }

        // Map domain DTO to EF entity (like lab pattern)
        var entity = new ShipmentEntity
        {
            ShipmentId = shipment.ShipmentId,
            OrderId = shipment.OrderId,
            UserId = shipment.UserId,
            TrackingNumber = shipment.TrackingNumber,
            TotalPrice = shipment.TotalPrice,
            ShippingCost = shipment.ShippingCost,
            TotalWithShipping = shipment.TotalWithShipping,
            Status = shipment.Status,
            CreatedAt = DateTime.UtcNow,
            Lines = shipment.Lines.Select(l => new ShipmentLineEntity
            {
                ShipmentLineId = l.ShipmentLineId,
                ShipmentId = shipment.ShipmentId,
                Name = l.Name,
                Description = l.Description,
                Category = l.Category,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                LineTotal = l.LineTotal
            }).ToList()
        };

        await _context.Shipments.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ShipmentQueryResult?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Shipments
            .AsNoTracking()
            .Include(s => s.Lines)
            .FirstOrDefaultAsync(s => s.OrderId == orderId, cancellationToken);

        if (entity == null)
            return null;

        // Map entity to domain model (like lab pattern)
        return MapToQueryResult(entity);
    }

    public async Task<ShipmentQueryResult?> GetByIdAsync(Guid shipmentId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Shipments
            .AsNoTracking()
            .Include(s => s.Lines)
            .FirstOrDefaultAsync(s => s.ShipmentId == shipmentId, cancellationToken);

        if (entity == null)
            return null;

        // Map entity to domain model (like lab pattern)
        return MapToQueryResult(entity);
    }

    public async Task UpdateStatusAsync(Guid shipmentId, string newStatus, CancellationToken cancellationToken = default)
    {
        var shipment = await _context.Shipments.FindAsync(new object[] { shipmentId }, cancellationToken);
        if (shipment != null)
        {
            shipment.Status = newStatus;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }


    public async Task UpdateStatusByOrderIdAsync(Guid orderId, string newStatus, CancellationToken cancellationToken = default)
    {
        var shipment = await _context.Shipments
            .FirstOrDefaultAsync(s => s.OrderId == orderId, cancellationToken);
        
        if (shipment != null)
        {
            shipment.Status = newStatus;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Maps EF entity to domain query result (lab pattern)
    /// </summary>
    private static ShipmentQueryResult MapToQueryResult(ShipmentEntity entity)
    {
        return new ShipmentQueryResult
        {
            ShipmentId = entity.ShipmentId,
            OrderId = entity.OrderId,
            UserId = entity.UserId,
            TrackingNumber = entity.TrackingNumber,
            TotalPrice = entity.TotalPrice,
            ShippingCost = entity.ShippingCost,
            TotalWithShipping = entity.TotalWithShipping,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt
        };
    }
}
