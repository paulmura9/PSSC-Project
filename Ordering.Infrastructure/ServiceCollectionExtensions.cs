using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Domain.Operations;
using Ordering.Domain.Repositories;
using Ordering.Infrastructure.Persistence;
using Ordering.Infrastructure.Repository;
using SharedKernel.Ordering;

namespace Ordering.Infrastructure;

/// <summary>
/// Extension methods for registering Infrastructure services
/// This allows Domain to stay clean while Api can reference Infrastructure for DI
/// </summary>
/// intregisrez serviciile 
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrderingInfrastructure(this IServiceCollection services, string connectionString)
    {
        // Configure EF Core with SQL Server
        services.AddDbContext<OrderingDbContext>(options =>
            options.UseSqlServer(connectionString));
        
        // Register repositories
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IVoucherRepository, VoucherRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        
        return services;
    }
}

