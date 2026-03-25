# DistributedOrderSystem

This project is an event-driven ASP.NET Core system built around RabbitMQ. When creating this project, i focused on reliable messaging, clear service boundaries, and correct handling of distributed workflows

Rather than trying to create a full production platform the scope is severely reduced, only focusing on asynchronous communication and eventual consistency using the outbox pattern 



## Overview

The system is composed of multiple services that communicate through RabbitMQ:
## Services

### OrderService

- Creates orders
- Stores OrderCreatedEvent in the outbox
- Publishes events via background worker

### PaymentService

- Creates pending payments from order events
- Completes payments via API
- Publishes PaymentCompletedEvent

### InventoryService

- Consumes payment events
- Updates product stock
- Prevents duplicate processing
Each service owns its own data and communicates using events


## Features

- Event-driven communication using RabbitMQ
- Outbox pattern for reliable message publishing
- Inventory updates driven by payment events
- Clear separation between controller, service, and messaging layers
- Manual message acknowledgements for safe processing
- Idempotent consumers using MessageId tracking
- Order creation with generated PaymentId
- Payment lifecycle with pending and completed states
- DTO-based request/response models


## Reliability

The system is designed around at-least-once delivery and eventual consistency.

### Idempotent Consumers

- Each message includes a MessageId
- Consumers track processed messages
- Duplicate messages are safely ignored

### Manual Acknowledgements

- Messages are acknowledged only after successful processing
- Failed messages are requeued automatically



## Tech Stack

- C#
- ASP.NET Core
- Entity Framework Core
- SQLite
- RabbitMQ
- BackgroundService


## Running the Application

### Start RabbitMQ

```bash
docker run -d --name rabbitmq \
-p 5672:5672 \
-p 15672:15672 \
rabbitmq:3-management
```

Management UI:

```
http://localhost:15672
guest / guest
```

---

### Run Services

Start each service individually:

```bash
dotnet run
```

---

### Flow

- POST `/orders` → creates order and returns PaymentId
- POST `/payments` → completes payment
 Inventory updates automatically via events

