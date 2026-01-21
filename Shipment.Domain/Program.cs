using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharedKernel;
using SharedKernel.ServiceBus;
using SharedKernel.Shipment;
using Shipment.Domain;
using Shipment.Domain.Handlers;
using Shipment.Domain.Workflows;
using Shipment.Infrastructure;
using Shipment.Infrastructure.Repository;

var builder = Host.CreateApplicationBuilder(args);

// Configure Service Bus - separate clients for orders (consume) and shipments (publish)
var ordersConnectionString = builder.Configuration["ServiceBus:OrdersConnectionString"];
var shipmentsConnectionString = builder.Configuration["ServiceBus:ShipmentsConnectionString"];

// Check if Service Bus is configured
var serviceBusConfigured = !string.IsNullOrWhiteSpace(ordersConnectionString) && 
                           !string.IsNullOrWhiteSpace(shipmentsConnectionString);

if (!serviceBusConfigured)
{
    Console.WriteLine("WARNING: ServiceBus connection strings not configured. Service Bus features disabled.");
    Console.WriteLine("  - ServiceBus:OrdersConnectionString: " + (string.IsNullOrWhiteSpace(ordersConnectionString) ? "NOT SET" : "OK"));
    Console.WriteLine("  - ServiceBus:ShipmentsConnectionString: " + (string.IsNullOrWhiteSpace(shipmentsConnectionString) ? "NOT SET" : "OK"));
    Console.WriteLine("Shipment service will start but won't process messages.");
}
else
{
    // Register ServiceBusClientFactory for consuming from orders topic and publishing to shipments
    builder.Services.AddSingleton(new ServiceBusClientFactory(ordersConnectionString!, shipmentsConnectionString!));
    
    // Register ServiceBusClient for consuming (used by OrderEventProcessor)
    builder.Services.AddSingleton(sp => sp.GetRequiredService<ServiceBusClientFactory>().OrdersClient);
    
    // Register IEventBus for publishing to shipments topic
    builder.Services.AddSingleton<IEventBus>(sp => 
        new AzureServiceBusEventBus(shipmentsConnectionString!, sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<AzureServiceBusEventBus>>()));
    
    // Register workflow and handler (AbstractEventHandler pattern)
    builder.Services.AddScoped<CreateShipmentWorkflow>();
    builder.Services.AddScoped<OrderPlacedEventHandler>();
    
    // Register background service
    builder.Services.AddHostedService<OrderEventProcessor>();
}

// Configure EF Core with Azure SQL using extension method
var dbConnectionString = builder.Configuration["ConnectionStrings:psscDB"];
if (!string.IsNullOrWhiteSpace(dbConnectionString))
{
    builder.Services.AddShipmentInfrastructure(dbConnectionString);
}
else
{
    Console.WriteLine("WARNING: ConnectionStrings:psscDB not configured. Database features disabled.");
}

// Register event history service for CSV logging
var csvPath = Path.Combine(AppContext.BaseDirectory, "shipment_event_history.csv");
builder.Services.AddSingleton<IEventHistoryService>(new CsvEventHistoryService(csvPath));

var host = builder.Build();

Console.WriteLine("Shipment Service starting...");
if (serviceBusConfigured)
{
    Console.WriteLine($"Listening on topic '{TopicNames.Orders}', subscription '{SubscriptionNames.OrderProcessor}'");
    Console.WriteLine($"Publishing to topic '{TopicNames.Shipments}'");
}
Console.WriteLine($"Event history CSV: {csvPath}");

host.Run();