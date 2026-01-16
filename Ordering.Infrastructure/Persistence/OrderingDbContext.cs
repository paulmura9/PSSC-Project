using Microsoft.EntityFrameworkCore;

namespace Ordering.Infrastructure.Persistence;

/// <summary>
/// Database context for the Ordering bounded context
/// </summary>
public class OrderingDbContext : DbContext
{
    public DbSet<OrderEntity> Orders { get; set; } = null!;
    public DbSet<OrderLineEntity> OrderLines { get; set; } = null!;

    public OrderingDbContext(DbContextOptions<OrderingDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Order entity
        modelBuilder.Entity<OrderEntity>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.DeliveryAddress).IsRequired().HasMaxLength(500);
            entity.Property(e => e.PostalCode).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Phone).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CardNumberMasked).IsRequired().HasMaxLength(20);
            entity.Property(e => e.TotalPrice).HasPrecision(18, 2);
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasMany(e => e.Lines)
                  .WithOne(e => e.Order)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure OrderLine entity
        modelBuilder.Entity<OrderLineEntity>(entity =>
        {
            entity.ToTable("OrderLines");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Quantity).IsRequired();
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.LineTotal).HasPrecision(18, 2);
        });
    }
}

