using Microsoft.EntityFrameworkCore;

namespace Ordering.Infrastructure.Persistence;

/// <summary>
/// Database context for the Ordering bounded context
/// Uses 'ordering' schema to isolate tables from other bounded contexts
/// </summary>
/// scaffolding 
public class OrderingDbContext : DbContext
{
    private const string Schema = "ordering";
    
    public DbSet<ProductEntity> Products { get; set; } = null!;
    public DbSet<OrderEntity> Orders { get; set; } = null!;
    public DbSet<OrderLineEntity> OrderLines { get; set; } = null!;
    public DbSet<VoucherEntity> Vouchers { get; set; } = null!;

    public OrderingDbContext(DbContextOptions<OrderingDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Set default schema for all entities in this context
        modelBuilder.HasDefaultSchema(Schema);

        // Configure Product entity
        modelBuilder.Entity<ProductEntity>(entity =>
        {
            entity.ToTable("Products", Schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.StockQuantity).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        // Configure Order entity
        modelBuilder.Entity<OrderEntity>(entity =>
        {
            entity.ToTable("Orders", Schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.Street).IsRequired().HasMaxLength(200);
            entity.Property(e => e.City).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PostalCode).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(254);
            entity.Property(e => e.DeliveryNotes).HasMaxLength(250);
            entity.Property(e => e.Subtotal).HasPrecision(18, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.Total).HasPrecision(18, 2);
            entity.Property(e => e.VoucherCode).HasMaxLength(64);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("Sent");
            entity.Property(e => e.CreatedAt).IsRequired();
            
            // Ignore TotalPrice - it's a computed property for backwards compatibility
            entity.Ignore(e => e.TotalPrice);

            entity.HasMany(e => e.Lines)
                  .WithOne(e => e.Order)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure OrderLine entity
        modelBuilder.Entity<OrderLineEntity>(entity =>
        {
            entity.ToTable("OrderLines", Schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.Quantity).IsRequired();
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.LineTotal).HasPrecision(18, 2);
        });

        // Configure Voucher entity
        modelBuilder.Entity<VoucherEntity>(entity =>
        {
            entity.ToTable("Vouchers", Schema);
            entity.HasKey(e => e.VoucherId);
            entity.Property(e => e.VoucherId).ValueGeneratedNever();
            entity.Property(e => e.Code).IsRequired().HasMaxLength(64);
            entity.Property(e => e.DiscountPercent).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            // MaxUses = remaining uses, NULL = unlimited
            entity.Property(e => e.MaxUses);
            
            // Unique index on Code
            entity.HasIndex(e => e.Code).IsUnique();
        });
    }
}

