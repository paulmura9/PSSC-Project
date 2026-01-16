using Microsoft.EntityFrameworkCore;
namespace Invoicing.Infrastructure.Persistence;
public class InvoicingDbContext : DbContext
{
    public InvoicingDbContext(DbContextOptions<InvoicingDbContext> options) : base(options) { }
    public DbSet<InvoiceEntity> Invoices { get; set; } = null!;
    public DbSet<InvoiceLineEntity> InvoiceLines { get; set; } = null!;
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<InvoiceEntity>(entity =>
        {
            entity.HasKey(e => e.InvoiceId);
            entity.Property(e => e.InvoiceNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TrackingNumber).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.SubTotal).HasPrecision(18, 2);
            entity.Property(e => e.Tax).HasPrecision(18, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.HasMany(e => e.Lines).WithOne(l => l.Invoice).HasForeignKey(l => l.InvoiceId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<InvoiceLineEntity>(entity =>
        {
            entity.HasKey(e => e.InvoiceLineId);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.LineTotal).HasPrecision(18, 2);
        });
    }
}
