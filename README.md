<<<<<<< HEAD
<<<<<<< HEAD
# Ordering.Api - DDD Workflow Implementation

This project implements a DDD (Domain-Driven Design) workflow for placing orders following the lab pattern with states, operations, results, and events.

## Architecture

The solution follows the lab pattern with:

- **Unvalidated -> Validated/Invalid -> Priced -> Persisted -> Published** state transitions
- Composed operations in a workflow
- Events published to Azure Service Bus

### Projects

- **Ordering.Api** - ASP.NET Core Web API with controllers and DTOs
- **Ordering.Domain** - Domain model with states, events, operations, and workflow
- **Ordering.Infrastructure** - EF Core persistence and Azure Service Bus publisher

## Domain States

1. `UnvalidatedOrder` - Initial state from user input
2. `InvalidOrder` - Order that failed validation
3. `ValidatedOrder` - Order that passed validation
4. `PricedOrder` - Order with calculated total price
5. `PersistedOrder` - Order saved to database
6. `PublishedOrder` - Order event published to Service Bus

## Configuration

### appsettings.json

Update the following settings in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "psscDB": "Server=YOUR_SERVER.database.windows.net;Database=psscDB;User Id=YOUR_USER;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=False;"
  },
  "ServiceBus": {
    "ConnectionString": "Endpoint=sb://YOUR_NAMESPACE.servicebus.windows.net/;SharedAccessKeyName=YOUR_KEY_NAME;SharedAccessKey=YOUR_KEY",
    "QueueName": "order-placed-events"
  }
}
```

## Database Migrations

### Creating the Initial Migration

Run the following commands from the solution root:

```powershell
# Install EF Core tools (if not already installed)
dotnet tool install --global dotnet-ef

# Create the initial migration
cd Ordering.Api
dotnet ef migrations add InitialCreate --project ..\Ordering.Infrastructure --context OrderingDbContext

# Apply the migration to the database
dotnet ef database update --project ..\Ordering.Infrastructure --context OrderingDbContext
```

### Database Tables

The migration will create:

- **Orders** - Main order table with Id, UserId, DeliveryAddress, PostalCode, Phone, CardNumberMasked, TotalPrice, CreatedAt
- **OrderLines** - Order lines with Id, OrderId, Name, Quantity, UnitPrice, LineTotal

**Note:** CVV is NOT stored in the database for security. Card numbers are masked (only last 4 digits stored).

## API Endpoints

### POST /api/orders/place

Place a new order.

**Request Body:**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "deliveryAddress": "123 Main Street",
  "postalCode": "12345",
  "phone": "+40721123456",
  "cardNumber": "4111111111111111",
  "cvv": "123",
  "expiry": "12/25",
  "products": [
    {
      "name": "Laptop",
      "quantity": 1,
      "unitPrice": 1299.99
    }
  ]
}
```

**Success Response (200 OK):**
```json
{
  "orderId": "guid",
  "totalPrice": 1299.99,
  "occurredAt": "2026-01-14T23:00:00Z",
  "lines": [
    {
      "name": "Laptop",
      "quantity": 1,
      "unitPrice": 1299.99,
      "lineTotal": 1299.99
    }
  ]
}
```

**Error Response (400 Bad Request):**
```json
{
  "errors": [
    "Delivery address is required",
    "At least one order line is required"
  ]
}
```

## Azure Service Bus Event

When an order is successfully placed, an event is published to Azure Service Bus:

```json
{
  "eventType": "OrderPlaced",
  "orderId": "guid",
  "userId": "guid",
  "totalPrice": 1299.99,
  "lines": [...],
  "occurredAt": "2026-01-14T23:00:00Z"
}
```

Message Properties:
- Subject: "OrderPlaced"
- ContentType: "application/json"
- MessageId: OrderId

## Running the Application

```powershell
cd Ordering.Api
dotnet run
```

The API will be available at:
- HTTP: http://localhost:5272
- Swagger UI: http://localhost:5272/swagger

=======
=======
>>>>>>> 9b88eaab0e93d9fb0901f760bb61ca04b90cce00
# PSSC Project

## Domain-Driven Design Based Distributed Workflow System

This project implements a distributed workflow system based on **Domain-Driven Design (DDD)** principles.  
The system is structured around three bounded contexts: **Ordering**, **Shipment**, and **Invoicing**, which communicate asynchronously through domain events.  
Each context encapsulates its own domain logic, workflows, and state transitions, following a clear separation of responsibilities.

The project is developed as part of the **PSSC laboratory**, focusing on workflow modeling, domain operations, and asynchronous communication between contexts.

---

## Team Members
- Mura Paul  
- Macovei Mark


<img width="1726" height="886" alt="image1" src="https://github.com/user-attachments/assets/723b127b-7d5c-4a3d-a4ea-1636910299fb" />
<<<<<<< HEAD
>>>>>>> 9b88eaab0e93d9fb0901f760bb61ca04b90cce00
=======
>>>>>>> 9b88eaab0e93d9fb0901f760bb61ca04b90cce00
