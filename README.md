
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


## Running Locally 

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
