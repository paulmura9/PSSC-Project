using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
namespace Invoicing.Infrastructure.Persistence;
public class InvoicingDbContextFactory : IDesignTimeDbContextFactory<InvoicingDbContext>
{
    public InvoicingDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();
        var optionsBuilder = new DbContextOptionsBuilder<InvoicingDbContext>();
        optionsBuilder.UseSqlServer(configuration.GetConnectionString("psscDB"));
        return new InvoicingDbContext(optionsBuilder.Options);
    }
}
