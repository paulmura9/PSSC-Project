﻿﻿﻿﻿﻿using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharedKernel;
using SharedKernel.Invoicing;
using SharedKernel.ServiceBus;
using Invoicing;
using Invoicing.Handlers;
using Invoicing.Operations;
using Invoicing.Workflows;
using Invoicing.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);

// Configure Service Bus - separate clients for shipments (consume) and invoices (publish)
var shipmentsConnectionString = builder.Configuration["ServiceBus:ShipmentsConnectionString"];
var invoicesConnectionString = builder.Configuration["ServiceBus:InvoicesConnectionString"];

// Check if Service Bus is configured
var serviceBusConfigured = !string.IsNullOrWhiteSpace(shipmentsConnectionString) && 
                           !string.IsNullOrWhiteSpace(invoicesConnectionString);

if (!serviceBusConfigured)
{
    Console.WriteLine("WARNING: ServiceBus connection strings not configured. Service Bus features disabled.");
    Console.WriteLine("  - ServiceBus:ShipmentsConnectionString: " + (string.IsNullOrWhiteSpace(shipmentsConnectionString) ? "NOT SET" : "OK"));
    Console.WriteLine("  - ServiceBus:InvoicesConnectionString: " + (string.IsNullOrWhiteSpace(invoicesConnectionString) ? "NOT SET" : "OK"));
    Console.WriteLine("Invoicing service will start but won't process messages.");
}
else
{
    // Register ServiceBusClientFactory for consuming from shipments topic and publishing to invoices
    builder.Services.AddSingleton(new ServiceBusClientFactory(shipmentsConnectionString!, invoicesConnectionString!));
    
    // Register IEventBus for publishing to invoices topic (like Shipment)
    builder.Services.AddSingleton<IEventBus>(sp => 
        new AzureServiceBusEventBus(invoicesConnectionString!, sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<AzureServiceBusEventBus>>()));
    
    // Register operations
    builder.Services.AddScoped<PersistInvoiceOperation>();
    builder.Services.AddScoped<PublishInvoiceOperation>();
    
    // Register workflow and handler (AbstractEventHandler pattern - like Shipment)
    builder.Services.AddScoped<CreateInvoiceWorkflow>();
    builder.Services.AddScoped<ShipmentStateChangedHandler>();
    
    // Register background service
    builder.Services.AddHostedService<ShipmentEventProcessor>();
}

// Configure EF Core with Azure SQL using extension method
var connectionString = builder.Configuration["ConnectionStrings:psscDB"];
if (!string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddInvoicingInfrastructure(connectionString);
}
else
{
    Console.WriteLine("WARNING: ConnectionStrings:psscDB not configured. Database features disabled.");
}

// Register event history service for CSV logging
var csvPath = Path.Combine(AppContext.BaseDirectory, "invoicing_event_history.csv");
builder.Services.AddSingleton<IEventHistoryService>(new CsvEventHistoryService(csvPath));


var host = builder.Build();

// Migrations are applied manually, not at startup
// To apply migrations run: dotnet ef database update --project Invoicing.Infrastructure

Console.WriteLine("Invoicing Service starting...");
if (serviceBusConfigured)
{
    Console.WriteLine($"Listening on topic '{TopicNames.Shipments}', subscription '{SubscriptionNames.ShipmentProcessor}'");
    Console.WriteLine($"Publishing to topic '{TopicNames.Invoices}'");
}
Console.WriteLine($"Event history CSV: {csvPath}");

host.Run();

