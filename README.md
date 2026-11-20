# 🏠 Paying Guest (PG) Management System

Production-grade, highly performant full-stack Paying Guest Management solution built with **ASP.NET Core 10 Web API** (clean architecture, CQRS, Observability, Rate Limiting, Hybrid Caching) and **Angular 22+** (standalone components, Signals, Reactive Forms, Material Design).

---

## ⚡ Tech Stack

| Layer | Technologies |
|---|---|
| **Backend API** | ASP.NET Core 10 / .NET 11, C# 13, Minimal APIs + Fast Controllers, MediatR CQRS |
| **Persistence** | SQL Server 2022, Entity Framework Core 10, Soft Deletes, Query Filters |
| **Caching** | FusionCache (L1 Memory + L2 Redis Hybrid Cache with Stampede Protection) |
| **Security** | ASP.NET Core Identity + JWT Bearer Tokens + Refresh Tokens |
| **Observability** | OpenTelemetry (Traces, Metrics, OTLP), Prometheus (`/metrics`), Jaeger, Grafana |
| **Logging** | Serilog with CorrelationId, MachineName, and daily rolling files |
| **Health Checks** | SQL Server, Redis, HealthChecks UI Dashboard (`/health/ui`) |
| **Resilience & Rate Limiting** | Polly v8, .NET 10 Built-in Rate Limiting (Fixed, Sliding, Token Bucket, Concurrency) |
| **PDF Generation** | QuestPDF for instant tenant invoice and dues receipt generation |
| **Frontend** | Angular 22+, Angular Material, Signals & `resource()`, Chart.js |

---

## 🚀 Quick Start

### 1. Prerequisites
- [.NET 10/11 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+ & npm](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/)

### 2. Start Supporting Services (Database, Cache & Observability)
Run docker-compose from the project root:
```bash
docker-compose up -d
```
This boots:
- **SQL Server 2022** on `localhost:1433`
- **Redis 7** on `localhost:6379`
- **Seq Log Viewer** on `http://localhost:5341`
- **Jaeger Tracing** on `http://localhost:16686`
- **Prometheus** on `http://localhost:9090`
- **Grafana** on `http://localhost:3000` (User: `admin`, Pass: `admin`)

### 3. Run Database Initialization
Execute the SQL script in `Database/init.sql` against your SQL Server instance, or run:
```bash
sqlcmd -S localhost -U sa -P "PGManagement@123!" -i Database/init.sql
```

### 4. Run the Web API (.NET 10)
```bash
cd src/PGManagement.API
dotnet run
```
API endpoints will be available at:
- **Scalar OpenAPI Docs:** `http://localhost:5000/scalar`
- **Health Check UI:** `http://localhost:5000/health/ui`
- **Readiness Probe:** `http://localhost:5000/health/ready`
- **Prometheus Metrics:** `http://localhost:5000/metrics`

### 5. Run the Angular Frontend
```bash
cd pg-management
npm install
npm start
```
Navigate to `http://localhost:4200/`.

---

## 📂 Project Architecture

```
PGManagement/
├── Database/
│   └── init.sql                       # Full database DDL, constraints, stored proc, seed data
├── src/
│   ├── PGManagement.Domain/           # Entities, Enums, Exceptions, BaseEntity
│   ├── PGManagement.Contracts/        # Shared Requests, Responses, ApiResponse envelope
│   ├── PGManagement.Infrastructure/   # EF Core DbContext, Repositories, FusionCache, Identity
│   ├── PGManagement.Application/      # CQRS Commands, Queries, MediatR, FluentValidation, Mapster
│   └── PGManagement.API/              # ASP.NET Core 10 Web API, Controllers, Middleware, QuestPDF
├── pg-management/                     # Angular 22+ Standalone Frontend
├── docker-compose.yml                 # Local dev stack (SQL Server, Redis, Seq, Jaeger, Prometheus)
└── PG-Management-System-Prompt.md     # Master prompt specification
```

---

## 🔒 API Endpoints & Policies

| Verb | Path | Auth | Rate Limit Policy | Description |
|---|---|---|---|---|
| `POST` | `/api/v1/auth/register` | None | Anonymous (30/min) | Register administrator / staff |
| `POST` | `/api/v1/auth/login` | None | Anonymous (30/min) | Login and get JWT token |
| `GET` | `/api/v1/dashboard` | Optional | Heavy Reports (Concurrency 5) | Occupancy, revenue, dues summary |
| `GET` | `/api/v1/rooms` | Optional | Authenticated (200/min) | Paginated rooms list |
| `GET` | `/api/v1/rooms/available` | None | Cached (2 min) | Available rooms and vacancies |
| `POST` | `/api/v1/rooms` | Bearer | Authenticated (200/min) | Register new room |
| `GET` | `/api/v1/tenants` | Optional | Authenticated (200/min) | List all tenants |
| `POST` | `/api/v1/tenants` | Bearer | Authenticated (200/min) | Register new tenant |
| `POST` | `/api/v1/allocations` | Bearer | Authenticated (200/min) | Check-in / Assign tenant to room |
| `PATCH` | `/api/v1/allocations/{id}/checkout`| Bearer | Authenticated (200/min) | Check-out tenant |
| `POST` | `/api/v1/bills/generate` | Bearer | Token Bucket (10/burst) | Generate monthly tenant bill |
| `GET` | `/api/v1/bills/pending-dues` | Optional | Authenticated (200/min) | Overdue dues by tenant |
| `GET` | `/api/v1/bills/{id}/print` | Optional | Feature: BillPrintingEnabled | Download QuestPDF invoice |
| `POST` | `/api/v1/payments` | Bearer | Authenticated (200/min) | Record tenant payment |
