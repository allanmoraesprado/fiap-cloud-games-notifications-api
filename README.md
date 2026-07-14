# FIAP Cloud Games — Notifications API

Notifications microservice for the FIAP Cloud Games Phase 2 platform. It is a
**Kafka consumer** that simulates sending e-mails by **logging them to the
console** (no SMTP, no real e-mail provider — intentional for this academic MVP).

Stateless .NET 8 consumer that **simulates e-mails by logging to the console** (no
SMTP). Consumes **`UserCreatedEvent`** (welcome) and **`PaymentProcessedEvent`**
(purchase confirmation, on **Approved** only), both under group
`notifications-service`. Runs via Docker Compose and on local Kubernetes (see
`k8s/`); for the full system runbook, see the
**`fiap-cloud-games-orchestration`** repository.

Part of the five-repository solution (`users-api`, `catalog-api`,
`payments-api`, `notifications-api`, `orchestration`).

---

## Responsibilities

- Consume `fcg.users.created` (group `notifications-service`) → log a welcome e-mail.
- Expose `/health` for liveness.

No database, no HTTP business endpoints, no JWT.

---

## Tech

.NET 8 · minimal ASP.NET Core `WebApplication` hosting a `BackgroundService`
consumer · `Confluent.Kafka` · Serilog · xUnit/FluentAssertions.

Single-project layout: `Consumers`, `Contracts`, `Messaging`.

---

## Events consumed

| Event | Topic | Consumer group | Action |
|---|---|---|---|
| `UserCreatedEvent` | `fcg.users.created` | `notifications-service` | Log a simulated welcome e-mail |
| `PaymentProcessedEvent` | `fcg.payments.processed` | `notifications-service` | Log a purchase confirmation (**Approved** only) |

Delivery is **at-least-once** (manual offset commit after handling); a duplicate
delivery may log the e-mail twice — acceptable for the MVP.

---

## Environment variables

| Variable | Meaning | Local default |
|---|---|---|
| `Kafka__BootstrapServers` | Kafka bootstrap (host dev / `kafka:9092` in containers) | `localhost:29092` |
| `Kafka__UserCreatedTopic` | Topic to consume | `fcg.users.created` |
| `Kafka__ConsumerGroup` | Consumer group id | `notifications-service` |

No secrets are used (local Kafka is PLAINTEXT).

---

## Run locally (uses the M0 Kafka)

1. Start the M0 infrastructure (orchestration repo): `docker compose up -d`.
2. Run the consumer:
   ```bash
   dotnet run --project src/NotificationsApi --urls http://localhost:8081
   ```
3. Register a user in UsersAPI → a `[WELCOME EMAIL]` line appears in this
   service's console within ~1 second.

## Test

```bash
dotnet test
```

## Docker

```bash
docker build -t fcg-notifications-api .
docker run --rm -p 8081:8080 \
  -e Kafka__BootstrapServers="host.docker.internal:29092" \
  fcg-notifications-api
```
