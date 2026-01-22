using Ordering.Domain.Operations;
using Ordering.Domain.Workflows;
using Ordering.Infrastructure;
using SharedKernel;
using SharedKernel.ServiceBus;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("psscDB");
if (!string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddOrderingInfrastructure(connectionString);
}
else
{
    Console.WriteLine("WARNING: ConnectionStrings:psscDB not configured. Database features disabled.");
}

// Register event bus (singleton - manages internal Service Bus client)
// Configurează Azure Service Bus
var serviceBusConnectionString = builder.Configuration["ServiceBus:ConnectionString"];
if (string.IsNullOrWhiteSpace(serviceBusConnectionString))
{
    Console.WriteLine("WARNING: ServiceBus:ConnectionString not configured. Using NoOpEventBus (events will not be sent).");
}
else
{
    builder.Services.AddSingleton<IEventBus, AzureServiceBusEventBus>();
}

// Register CSV event history service for local tracking
var csvPath = Path.Combine(AppContext.BaseDirectory, "ordering_event_history.csv");
builder.Services.AddSingleton<IEventHistoryService>(new CsvEventHistoryService(csvPath));
Console.WriteLine($"========================================");
Console.WriteLine($"Event history CSV will be saved to:");
Console.WriteLine($"  {csvPath}");
Console.WriteLine($"========================================");



// Register operations
builder.Services.AddScoped<ValidateOrderOperation>();
builder.Services.AddScoped<ApplyVoucherOperation>();
builder.Services.AddScoped<PriceOrderOperation>();
builder.Services.AddScoped<PersistOrderOperation>();
builder.Services.AddScoped<PublishOrderPlacedOperation>();

// Register workflows
builder.Services.AddScoped<PlaceOrderWorkflow>();

var app = builder.Build();

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