using Microsoft.EntityFrameworkCore;
using Ordering.Domain.Operations;
using Ordering.Domain.Workflows;
using Ordering.Infrastructure.Persistence;
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

// Register event bus (singleton - manages internal Service Bus client)
builder.Services.AddSingleton<IEventBus, AzureServiceBusEventBus>();

// Register operations
builder.Services.AddScoped<ValidateOrderOperation>();
builder.Services.AddScoped<PriceOrderOperation>();
builder.Services.AddScoped<PersistOrderOperation>();

// Register workflow
builder.Services.AddScoped<PlaceOrderWorkflow>();

var app = builder.Build();

// Apply database migrations automatically at startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
    dbContext.Database.Migrate();
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