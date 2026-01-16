using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Shipment.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for EF Core migrations
/// </summary>
public class ShipmentDbContextFactory : IDesignTimeDbContextFactory<ShipmentDbContext>
{
    public ShipmentDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString = configuration.GetConnectionString("psscDB")
            ?? throw new InvalidOperationException("ConnectionString 'psscDB' is not configured");

        var optionsBuilder = new DbContextOptionsBuilder<ShipmentDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new ShipmentDbContext(optionsBuilder.Options);
    }
}

