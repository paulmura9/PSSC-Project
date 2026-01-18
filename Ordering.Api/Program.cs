using Microsoft.EntityFrameworkCore;
using Ordering.Domain.Operations;
using Ordering.Domain.Workflows;
using Ordering.Infrastructure.Persistence;
using Ordering.Infrastructure.Repository;
using SharedKernel;
using SharedKernel.ServiceBus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure EF Core with Azure SQL
builder.Services.AddDbContext<OrderingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("psscDB")));

// Register persistence
builder.Services.AddScoped<IPersistence, EfCorePersistence>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IVoucherRepository, VoucherRepository>();

// Register event bus (singleton - manages internal Service Bus client)
builder.Services.AddSingleton<IEventBus, AzureServiceBusEventBus>();

// Register CSV event history service for local tracking
var csvPath = Path.Combine(AppContext.BaseDirectory, "ordering_event_history.csv");
builder.Services.AddSingleton<IEventHistoryService>(new CsvEventHistoryService(csvPath));
Console.WriteLine($"Event history CSV: {csvPath}");

// Register event publisher
builder.Services.AddScoped<IOrderEventPublisher, ServiceBusOrderEventPublisher>();

// Register repository for order queries
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Register operations
builder.Services.AddScoped<ValidateOrderOperation>();
builder.Services.AddScoped<ApplyVoucherOperation>();
builder.Services.AddScoped<PriceOrderOperation>();
builder.Services.AddScoped<MakePersistableOrderOperation>();
builder.Services.AddScoped<PersistOrderOperation>();
builder.Services.AddScoped<PublishOrderPlacedOperation>();

// Register workflows
builder.Services.AddScoped<PlaceOrderWorkflow>();
builder.Services.AddScoped<CancelOrderWorkflow>();
builder.Services.AddScoped<ReturnOrderWorkflow>();

var app = builder.Build();

// Run database migrations for new columns
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
    try
    {
        Console.WriteLine("Checking for pending column migrations...");
        
        // Add missing columns using raw SQL
        var migrations = new[]
        {
            "IF COL_LENGTH('ordering.Orders', 'PickupMethod') IS NULL ALTER TABLE [ordering].[Orders] ADD [PickupMethod] NVARCHAR(32) NOT NULL DEFAULT 'HomeDelivery'",
            "IF COL_LENGTH('ordering.Orders', 'PickupPointId') IS NULL ALTER TABLE [ordering].[Orders] ADD [PickupPointId] NVARCHAR(64) NULL",
            "IF COL_LENGTH('ordering.Orders', 'PaymentMethod') IS NULL ALTER TABLE [ordering].[Orders] ADD [PaymentMethod] NVARCHAR(32) NOT NULL DEFAULT 'CashOnDelivery'"
        };

        foreach (var sql in migrations)
        {
            await context.Database.ExecuteSqlRawAsync(sql);
        }
        
        Console.WriteLine("Database column migrations completed!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration warning: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();