using Microsoft.EntityFrameworkCore;

namespace Shipment.Infrastructure.Persistence;

/// <summary>
/// Database context for Shipment microservice
/// Uses 'shipment' schema to isolate tables from other bounded contexts
/// </summary>
public class ShipmentDbContext : DbContext
{
    private const string Schema = "shipment";

    public ShipmentDbContext(DbContextOptions<ShipmentDbContext> options)
        : base(options)
    {
    }

    public DbSet<ShipmentEntity> Shipments { get; set; } = null!;
    public DbSet<ShipmentLineEntity> ShipmentLines { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Set default schema for all entities in this context
        modelBuilder.HasDefaultSchema(Schema);

        // Configure Shipment entity
        modelBuilder.Entity<ShipmentEntity>(entity =>
        {
            entity.ToTable("Shipments", Schema);
            entity.HasKey(e => e.ShipmentId);
            entity.Property(e => e.ShipmentId).ValueGeneratedNever();
            entity.Property(e => e.OrderId).IsRequired();
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.TotalPrice).IsRequired().HasPrecision(18, 2);
            entity.Property(e => e.ShippingCost).IsRequired().HasPrecision(18, 2);
            entity.Property(e => e.TotalWithShipping).IsRequired().HasPrecision(18, 2);
            entity.Property(e => e.TrackingNumber).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedAt).IsRequired();
            
            // Create index for faster lookups
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.TrackingNumber).IsUnique();

            // Configure relationship with lines
            entity.HasMany(e => e.Lines)
                .WithOne(l => l.Shipment)
                .HasForeignKey(l => l.ShipmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure ShipmentLine entity
        modelBuilder.Entity<ShipmentLineEntity>(entity =>
        {
            entity.ToTable("ShipmentLines", Schema);
            entity.HasKey(e => e.ShipmentLineId);
            entity.Property(e => e.ShipmentLineId).ValueGeneratedNever();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.Quantity).IsRequired();
            entity.Property(e => e.UnitPrice).IsRequired().HasPrecision(18, 2);
            entity.Property(e => e.LineTotal).IsRequired().HasPrecision(18, 2);

            entity.HasIndex(e => e.ShipmentId);
        });
    }
}

