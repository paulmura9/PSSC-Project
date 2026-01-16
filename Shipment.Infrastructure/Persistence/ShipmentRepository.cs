using Microsoft.EntityFrameworkCore;

namespace Shipment.Infrastructure.Persistence;

/// <summary>
/// Interface for Shipment persistence operations
/// </summary>
public interface IShipmentRepository
{
    Task SaveShipmentAsync(ShipmentEntity shipment, IEnumerable<ShipmentLineEntity> lines, CancellationToken cancellationToken = default);
    Task<ShipmentEntity?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository implementation for Shipment persistence operations
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
}
