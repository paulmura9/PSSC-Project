
# PSSC Project

## Domain-Driven Design Based Distributed Workflow System

This project implements a distributed workflow system based on **Domain-Driven Design (DDD)** principles.  
The system is structured around three bounded contexts: **Ordering**, **Shipment**, and **Invoicing**, which communicate asynchronously through domain events.  
Each context encapsulates its own domain logic, workflows, and state transitions, following a clear separation of responsibilities.

The project is developed as part of the **PSSC laboratory**, focusing on workflow modeling, domain operations, and asynchronous communication between contexts.

---

## Architecture

```
┌─────────────────┐       ┌─────────────────┐       ┌─────────────────┐
│   Ordering.Api  │──────►│ Shipment.Domain │──────►│Invoicing.Domain │
│   (HTTP API)    │       │  (Background)   │       │  (Background)   │
└─────────────────┘       └─────────────────┘       └─────────────────┘
        │                         │                         │
        ▼                         ▼                         ▼
   ┌─────────┐              ┌─────────┐              ┌─────────┐
   │ orders  │              │shipments│              │invoices │
   │ (topic) │              │ (topic) │              │ (topic) │
   └─────────┘              └─────────┘              └─────────┘
        │                         │                         │
        └─────────────────────────┴─────────────────────────┘
                           Azure Service Bus
```

---

## Running with Docker

### Prerequisites
- Docker Desktop installed
- Azure Service Bus namespace with topics: `orders`, `shipments`, `invoices`
- Azure SQL Database or SQL Server

### Quick Start

1. **Copy environment file:**
   ```bash
   cp .env.example .env
   ```

2. **Edit `.env` with your connection strings:**
   ```env
   DB_CONNECTION_STRING=Server=...;Database=pssc-db;...
   SERVICEBUS_ORDERS_CONNECTION=Endpoint=sb://...
   SERVICEBUS_SHIPMENTS_CONNECTION=Endpoint=sb://...
   SERVICEBUS_INVOICES_CONNECTION=Endpoint=sb://...
   ```

3. **Build and run all services:**
   ```bash
   docker-compose up --build
   ```

4. **Access Ordering API:**
   - Swagger UI: http://localhost:5272/swagger
   - API Endpoint: http://localhost:5272/api/orders

### Docker Services

| Service | Description | Port |
|---------|-------------|------|
| `ordering-api` | HTTP API for placing orders | 5272 |
| `shipment-service` | Listens to order events, creates shipments | - |
| `invoicing-service` | Listens to shipment events, creates invoices | - |

### Useful Commands

```bash
# Build all images
docker-compose build

# Run in background
docker-compose up -d

# View logs
docker-compose logs -f

# View specific service logs
docker-compose logs -f ordering-api

# Stop all services
docker-compose down

# Rebuild and restart
docker-compose up --build --force-recreate
```

---

## Running Locally (without Docker)

### Start all services in separate terminals:

```bash
# Terminal 1 - Ordering API
cd Ordering.Api
dotnet run

# Terminal 2 - Shipment Service
cd Shipment.Domain
dotnet run

# Terminal 3 - Invoicing Service
cd Invoicing.Domain
dotnet run
```

---

## Team Members
- Mura Paul  
- Macovei Mark


<img width="1726" height="886" alt="image1" src="https://github.com/user-attachments/assets/723b127b-7d5c-4a3d-a4ea-1636910299fb" />
