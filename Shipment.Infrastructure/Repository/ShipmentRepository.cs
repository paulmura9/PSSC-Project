using Microsoft.EntityFrameworkCore;

namespace Shipment.Infrastructure.Persistence;

/// <summary>
/// Repository implementation for Shipment persistence operations
/// Shipment reacts to events from Ordering
/// </summary>
public class ShipmentRepository : IShipmentRepository
{
    private readonly ShipmentDbContext _context;

    public ShipmentRepository(ShipmentDbContext context)
    {
        _context = context;
    }

    public async Task SaveShipmentAsync(ShipmentEntity shipment, IEnumerable<ShipmentLineEntity> lines, CancellationToken cancellationToken = default)
    {
        shipment.Lines = lines.ToList();
        await _context.Shipments.AddAsync(shipment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ShipmentEntity?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.Shipments
            .Include(s => s.Lines)
            .FirstOrDefaultAsync(s => s.OrderId == orderId, cancellationToken);
    }

    public async Task<ShipmentEntity?> GetByIdAsync(Guid shipmentId, CancellationToken cancellationToken = default)
    {
        return await _context.Shipments
            .Include(s => s.Lines)
            .FirstOrDefaultAsync(s => s.ShipmentId == shipmentId, cancellationToken);
    }

    public async Task UpdateStatusAsync(Guid shipmentId, string newStatus, CancellationToken cancellationToken = default)
    {
        var shipment = await _context.Shipments.FindAsync(new object[] { shipmentId }, cancellationToken);
        if (shipment != null)
        {
            shipment.Status = newStatus;
            shipment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task CancelByOrderIdAsync(Guid orderId, string reason, CancellationToken cancellationToken = default)
    {
        var shipment = await _context.Shipments
            .FirstOrDefaultAsync(s => s.OrderId == orderId, cancellationToken);
        
        if (shipment != null)
        {
            // Business rule: Can only cancel if not yet dispatched
            var cancellableStatuses = new[] { "Created", "Validated", "Scheduled", "Pending" };
            
            if (cancellableStatuses.Contains(shipment.Status))
            {
                shipment.Status = "Cancelled";
                shipment.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }

    public async Task UpdateStatusByOrderIdAsync(Guid orderId, string newStatus, CancellationToken cancellationToken = default)
    {
        var shipment = await _context.Shipments
            .FirstOrDefaultAsync(s => s.OrderId == orderId, cancellationToken);
        
        if (shipment != null)
        {
            shipment.Status = newStatus;
            shipment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
