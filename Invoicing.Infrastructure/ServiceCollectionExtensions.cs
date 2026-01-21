using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Invoicing;
using Invoicing.Infrastructure.Persistence;
using Invoicing.Infrastructure.Repository;

namespace Invoicing.Infrastructure;

/// <summary>
/// Extension methods for registering Infrastructure services
/// This allows Domain to stay clean while Api/App can reference Infrastructure for DI
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInvoicingInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<InvoicingDbContext>(options =>
            options.UseSqlServer(connectionString));
        
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        
        return services;
    }
}

