using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharedKernel;
using Shipment.Domain;
using Shipment.Infrastructure.Persistence;

var builder = Host.CreateApplicationBuilder(args);

// Configure Service Bus - separate clients for orders (consume) and shipments (publish)
var ordersConnectionString = builder.Configuration["ServiceBus:OrdersConnectionString"]
    ?? throw new InvalidOperationException("ServiceBus:OrdersConnectionString is not configured");
var shipmentsConnectionString = builder.Configuration["ServiceBus:ShipmentsConnectionString"]
    ?? throw new InvalidOperationException("ServiceBus:ShipmentsConnectionString is not configured");

builder.Services.AddSingleton(new ServiceBusClientFactory(ordersConnectionString, shipmentsConnectionString));

// Configure EF Core with Azure SQL
var connectionString = builder.Configuration["ConnectionStrings:psscDB"]
    ?? throw new InvalidOperationException("ConnectionString 'psscDB' is not configured");
builder.Services.AddDbContext<ShipmentDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register persistence
builder.Services.AddScoped<IShipmentRepository, ShipmentRepository>();

// Register background service (Service Bus trigger)
builder.Services.AddHostedService<OrderEventProcessor>();

var host = builder.Build();

Console.WriteLine("Shipment Service starting...");
Console.WriteLine($"Listening on topic '{TopicNames.Orders}', subscription '{SubscriptionNames.OrderProcessor}'");
Console.WriteLine($"Publishing to topic '{TopicNames.Shipments}'");

host.Run();
