﻿﻿using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharedKernel;
using Invoicing;
using Invoicing.Operations;
using Invoicing.Workflows;
using Invoicing.Infrastructure.Persistence;

var builder = Host.CreateApplicationBuilder(args);

// Configure Service Bus - separate clients for shipments (consume) and invoices (publish)
var shipmentsConnectionString = builder.Configuration["ServiceBus:ShipmentsConnectionString"]
    ?? throw new InvalidOperationException("ServiceBus:ShipmentsConnectionString is not configured");
var invoicesConnectionString = builder.Configuration["ServiceBus:InvoicesConnectionString"]
    ?? throw new InvalidOperationException("ServiceBus:InvoicesConnectionString is not configured");

builder.Services.AddSingleton(new ServiceBusClientFactory(shipmentsConnectionString, invoicesConnectionString));

// Configure EF Core with Azure SQL
var connectionString = builder.Configuration["ConnectionStrings:psscDB"]
    ?? throw new InvalidOperationException("ConnectionString 'psscDB' is not configured");
builder.Services.AddDbContext<InvoicingDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register persistence
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();

// Register operations
builder.Services.AddSingleton<ValidateInvoiceOperation>();
builder.Services.AddSingleton<GenerateInvoiceOperation>();

// Register workflow
builder.Services.AddSingleton<CreateInvoiceWorkflow>();

// Register background service (Service Bus trigger)
builder.Services.AddHostedService<ShipmentEventProcessor>();

var host = builder.Build();

// Migrations are applied manually, not at startup
// To apply migrations run: dotnet ef database update --project Invoicing.Infrastructure

Console.WriteLine("Invoicing Service starting...");
Console.WriteLine($"Listening on topic '{TopicNames.Shipments}', subscription '{SubscriptionNames.ShipmentProcessor}'");
Console.WriteLine($"Publishing to topic '{TopicNames.Invoices}'");

host.Run();

