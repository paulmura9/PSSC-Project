using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Shipment;
using Shipment.Infrastructure.Persistence;
using Shipment.Infrastructure.Repository;

namespace Shipment.Infrastructure;

/// <summary>
/// Extension methods for registering Infrastructure services
/// This allows Domain to stay clean while Api/App can reference Infrastructure for DI
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddShipmentInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ShipmentDbContext>(options =>
            options.UseSqlServer(connectionString));
        
        services.AddScoped<IShipmentRepository, ShipmentRepository>();
        
        return services;
    }
}

