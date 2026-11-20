# 🏠 Paying Guest Management System — Full-Stack Master Prompt

## Tech Stack
- **Frontend:** Angular v22+ (Standalone Components, Signals, SSR-ready)
- **Backend:** ASP.NET Core Web API (.NET 10)
- **Database:** SQL Server (latest)
- **ORM:** Entity Framework Core 10
- **Auth:** ASP.NET Core Identity + JWT Bearer Tokens

---

## 🎯 Project Overview

Build a **production-grade Paying Guest (PG) Management System** with the following core features:

| Feature | Description |
|---|---|
| Room Registration | Register rooms with type, capacity, amenities, rent |
| Room Availability | Show empty/occupied rooms in real time |
| Tenant Management | Onboard/offboard tenants, assign rooms |
| Dues & Payments | Track monthly dues, pending balances |
| Bill Generation | Generate and print itemized bills per tenant |
| Dashboard | Occupancy stats, revenue, overdue summary |

---

## 🗄️ SQL Server Database Design

### Guidelines
- Use `nvarchar` for string columns with appropriate max lengths
- All PKs are `int IDENTITY(1,1)` or `uniqueidentifier` (choose one consistently)
- All tables must have `CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()` and `UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()`
- Use `IsDeleted BIT NOT NULL DEFAULT 0` for soft deletes
- Add appropriate indexes on FK columns and frequently queried columns
- Use check constraints for enum-like columns

### Required Tables

```sql
-- 1. Rooms
CREATE TABLE Rooms (
    RoomId          INT IDENTITY(1,1) PRIMARY KEY,
    RoomNumber      NVARCHAR(20)   NOT NULL UNIQUE,
    Floor           INT            NOT NULL,
    RoomType        NVARCHAR(50)   NOT NULL,          -- Single, Double, Triple, Dormitory
    Capacity        INT            NOT NULL,
    MonthlyRent     DECIMAL(10,2)  NOT NULL,
    Amenities       NVARCHAR(500)  NULL,               -- JSON or comma-separated
    Status          NVARCHAR(20)   NOT NULL DEFAULT 'Available', -- Available, Occupied, Maintenance
    IsDeleted       BIT            NOT NULL DEFAULT 0,
    CreatedAt       DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT CK_Room_Status CHECK (Status IN ('Available','Occupied','Maintenance')),
    CONSTRAINT CK_Room_Capacity CHECK (Capacity > 0)
);

-- 2. Tenants
CREATE TABLE Tenants (
    TenantId        INT IDENTITY(1,1) PRIMARY KEY,
    FirstName       NVARCHAR(100)  NOT NULL,
    LastName        NVARCHAR(100)  NOT NULL,
    Email           NVARCHAR(255)  NOT NULL UNIQUE,
    Phone           NVARCHAR(20)   NOT NULL,
    IdProofType     NVARCHAR(50)   NOT NULL,           -- Aadhar, Passport, DL
    IdProofNumber   NVARCHAR(100)  NOT NULL,
    EmergencyContact NVARCHAR(200) NULL,
    ProfilePhotoUrl NVARCHAR(500)  NULL,
    IsActive        BIT            NOT NULL DEFAULT 1,
    IsDeleted       BIT            NOT NULL DEFAULT 0,
    CreatedAt       DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2      NOT NULL DEFAULT GETUTCDATE()
);

-- 3. Room Allocations (many tenants can share a room up to capacity)
CREATE TABLE RoomAllocations (
    AllocationId    INT IDENTITY(1,1) PRIMARY KEY,
    RoomId          INT            NOT NULL REFERENCES Rooms(RoomId),
    TenantId        INT            NOT NULL REFERENCES Tenants(TenantId),
    CheckInDate     DATE           NOT NULL,
    CheckOutDate    DATE           NULL,
    SecurityDeposit DECIMAL(10,2)  NOT NULL DEFAULT 0,
    Status          NVARCHAR(20)   NOT NULL DEFAULT 'Active', -- Active, CheckedOut
    CreatedAt       DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    INDEX IX_RoomAllocations_RoomId (RoomId),
    INDEX IX_RoomAllocations_TenantId (TenantId)
);

-- 4. Bills
CREATE TABLE Bills (
    BillId          INT IDENTITY(1,1) PRIMARY KEY,
    AllocationId    INT            NOT NULL REFERENCES RoomAllocations(AllocationId),
    BillMonth       INT            NOT NULL,            -- 1-12
    BillYear        INT            NOT NULL,
    RentAmount      DECIMAL(10,2)  NOT NULL,
    ElectricityCharges DECIMAL(10,2) NOT NULL DEFAULT 0,
    WaterCharges    DECIMAL(10,2)  NOT NULL DEFAULT 0,
    OtherCharges    DECIMAL(10,2)  NOT NULL DEFAULT 0,
    Discount        DECIMAL(10,2)  NOT NULL DEFAULT 0,
    TotalAmount     AS (RentAmount + ElectricityCharges + WaterCharges + OtherCharges - Discount) PERSISTED,
    DueDate         DATE           NOT NULL,
    GeneratedAt     DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    Notes           NVARCHAR(500)  NULL,
    CONSTRAINT CK_Bill_Month CHECK (BillMonth BETWEEN 1 AND 12),
    UNIQUE (AllocationId, BillMonth, BillYear),
    INDEX IX_Bills_AllocationId (AllocationId)
);

-- 5. Payments
CREATE TABLE Payments (
    PaymentId       INT IDENTITY(1,1) PRIMARY KEY,
    BillId          INT            NOT NULL REFERENCES Bills(BillId),
    AmountPaid      DECIMAL(10,2)  NOT NULL,
    PaymentDate     DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    PaymentMode     NVARCHAR(50)   NOT NULL,           -- Cash, UPI, BankTransfer, Card
    TransactionRef  NVARCHAR(200)  NULL,
    ReceivedBy      NVARCHAR(100)  NULL,
    Remarks         NVARCHAR(500)  NULL,
    CONSTRAINT CK_Payment_Amount CHECK (AmountPaid > 0),
    INDEX IX_Payments_BillId (BillId)
);

-- 6. Expenses (optional — operational costs for the PG owner)
CREATE TABLE Expenses (
    ExpenseId       INT IDENTITY(1,1) PRIMARY KEY,
    Category        NVARCHAR(100)  NOT NULL,
    Amount          DECIMAL(10,2)  NOT NULL,
    Description     NVARCHAR(500)  NULL,
    ExpenseDate     DATE           NOT NULL,
    CreatedAt       DATETIME2      NOT NULL DEFAULT GETUTCDATE()
);
```

---

## 🔧 ASP.NET Core 10 Web API — Backend

### Project Structure
```
PGManagement.sln
├── src/
│   ├── PGManagement.API/              # Web API host
│   │   ├── Controllers/v1/
│   │   ├── Controllers/v2/            # API versioning
│   │   ├── Middleware/
│   │   │   ├── ExceptionHandlingMiddleware.cs
│   │   │   └── RequestLoggingMiddleware.cs
│   │   ├── Extensions/
│   │   │   ├── ServiceCollectionExtensions.cs
│   │   │   └── WebApplicationExtensions.cs
│   │   └── Program.cs
│   ├── PGManagement.Application/      # CQRS, MediatR, Use Cases
│   │   ├── Commands/
│   │   ├── Queries/
│   │   ├── DTOs/
│   │   ├── Validators/                # FluentValidation
│   │   └── Mappings/                  # AutoMapper / Mapster
│   ├── PGManagement.Domain/           # Entities, Enums, Domain Events
│   │   ├── Entities/
│   │   ├── Enums/
│   │   └── Exceptions/
│   ├── PGManagement.Infrastructure/   # EF Core, Repos, External Services
│   │   ├── Persistence/
│   │   │   ├── AppDbContext.cs
│   │   │   ├── Configurations/        # IEntityTypeConfiguration
│   │   │   └── Migrations/
│   │   ├── Repositories/
│   │   ├── Caching/
│   │   └── Resilience/
│   └── PGManagement.Contracts/        # Shared request/response models
└── tests/
    ├── PGManagement.UnitTests/
    ├── PGManagement.IntegrationTests/
    └── PGManagement.ArchitectureTests/
```

### Required NuGet Packages
```xml
<!-- API & Hosting -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.*" />
<PackageReference Include="Scalar.AspNetCore" Version="*" />             <!-- Modern OpenAPI UI -->
<PackageReference Include="Asp.Versioning.Http" Version="*" />
<PackageReference Include="Asp.Versioning.ApiExplorer" Version="*" />

<!-- EF Core & SQL Server -->
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="10.*" />

<!-- CQRS -->
<PackageReference Include="MediatR" Version="*" />

<!-- Validation -->
<PackageReference Include="FluentValidation.AspNetCore" Version="*" />

<!-- Mapping -->
<PackageReference Include="Mapster" Version="*" />

<!-- Caching -->
<PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="*" />
<PackageReference Include="FusionCache" Version="*" />                    <!-- L1/L2 hybrid cache -->

<!-- Authentication -->
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.*" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.*" />

<!-- Health Checks -->
<PackageReference Include="AspNetCore.HealthChecks.SqlServer" Version="*" />
<PackageReference Include="AspNetCore.HealthChecks.Redis" Version="*" />
<PackageReference Include="AspNetCore.HealthChecks.UI" Version="*" />

<!-- Observability / Metrics -->
<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="*" />
<PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="*" />
<PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="*" />
<PackageReference Include="OpenTelemetry.Exporter.Otlp" Version="*" />
<PackageReference Include="OpenTelemetry.Instrumentation.SqlClient" Version="*" />
<PackageReference Include="prometheus-net.AspNetCore" Version="*" />

<!-- Logging -->
<PackageReference Include="Serilog.AspNetCore" Version="*" />
<PackageReference Include="Serilog.Sinks.Seq" Version="*" />
<PackageReference Include="Serilog.Sinks.Console" Version="*" />
<PackageReference Include="Serilog.Enrichers.CorrelationId" Version="*" />

<!-- Rate Limiting -->
<!-- Built-in: Microsoft.AspNetCore.RateLimiting (no extra package needed in .NET 10) -->

<!-- Resilience -->
<PackageReference Include="Microsoft.Extensions.Http.Resilience" Version="*" />
<PackageReference Include="Polly" Version="*" />

<!-- Feature Management -->
<PackageReference Include="Microsoft.FeatureManagement.AspNetCore" Version="*" />

<!-- PDF / Bill Printing -->
<PackageReference Include="QuestPDF" Version="*" />
```

### Key Implementation Requirements

#### 1. Exception Handling Middleware
```csharp
// Use the new .NET 10 IExceptionHandler interface
// Map domain exceptions to ProblemDetails (RFC 9457)
// Return structured JSON errors with:
//   - type (URI), title, status, detail, instance, traceId
// Never expose stack traces in production
// Log all 5xx errors with full context via Serilog
```

#### 2. API Versioning
```csharp
// Use Asp.Versioning with URL segment versioning: /api/v1/rooms, /api/v2/rooms
// Support header-based versioning as fallback: api-version: 1.0
// Deprecate old versions gracefully with Sunset and Deprecation headers
// Generate separate OpenAPI docs per version
```

#### 3. Rate Limiting (Built-in .NET 10)
```csharp
// Apply policies:
//   - Fixed window: anonymous users → 30 req/min
//   - Sliding window: authenticated users → 200 req/min
//   - Token bucket: bill generation endpoint → 10 req/min
//   - Concurrency: heavy report endpoints → max 5 concurrent
// Return 429 Too Many Requests with Retry-After header
```

#### 4. Caching Strategy
```csharp
// Use FusionCache for L1 (in-memory) + L2 (Redis) hybrid caching
// Cache keys should be deterministic and versioned
// Cache:
//   - Room list: 5 minutes (invalidate on room status change)
//   - Availability dashboard: 2 minutes
//   - Tenant list: 10 minutes
//   - Bill details: 30 minutes (immutable after generation)
// Use cache-aside pattern
// Implement cache stampede protection via FusionCache's built-in locking
```

#### 5. Health Checks
```csharp
// Endpoints:
//   GET /health          → liveness (is the app running?)
//   GET /health/ready    → readiness (can it serve traffic? checks DB, Redis)
//   GET /health/live     → startup probe
//   GET /health/ui       → Health Checks UI dashboard
// Check: SQL Server connectivity, Redis, disk space, memory pressure
// Return JSON with status per component
```

#### 6. Observability (OpenTelemetry)
```csharp
// Traces: instrument all HTTP requests, DB queries (EF Core + SqlClient), Redis calls
// Export to OTLP (Jaeger / Tempo)
// Metrics: expose /metrics endpoint (Prometheus scrape target)
//   - Custom metrics: active_tenants_total, monthly_revenue_gauge, 
//     pending_dues_total, room_occupancy_ratio
// Logs: structured JSON via Serilog → Seq / Loki
// Correlate logs, traces with TraceId / SpanId automatically
// Add custom activity source: "PGManagement.API"
```

#### 7. Logging
```csharp
// Use Serilog with:
//   - Enrichers: CorrelationId, MachineName, Environment, ThreadId
//   - Sinks: Console (dev), Seq (staging/prod), File (rolling daily)
//   - Minimum level: Information (Warning in prod for performance)
// Log request/response (exclude sensitive fields: passwords, tokens)
// Log slow queries (> 500ms) as warnings
// Structured log properties: UserId, TenantId, RoomId, BillId per request
```

#### 8. Resilience (Polly v8 + Microsoft.Extensions.Http.Resilience)
```csharp
// For outgoing HTTP calls and DB operations:
//   - Retry: exponential backoff, 3 retries, jitter
//   - Circuit Breaker: open after 5 failures in 30s, half-open after 60s
//   - Timeout: 10s per operation, 30s overall
//   - Bulkhead: limit concurrent DB calls
// Use ResiliencePipelineBuilder for composing strategies
```

#### 9. Feature Management
```csharp
// Feature flags (appsettings.json + Azure App Config / LaunchDarkly):
//   - "BillPrintingEnabled": toggle PDF bill generation
//   - "OnlineDuesPayment": toggle payment gateway integration
//   - "AdvancedReporting": toggle dashboard charts
//   - "TenantSelfPortal": toggle tenant-facing UI routes
// Use [FeatureGate("FeatureName")] on controllers/actions
// Return 404 or 503 gracefully when feature is disabled
```

### API Endpoints (v1)

```
# Rooms
GET    /api/v1/rooms                    → List all rooms (paginated, filterable)
GET    /api/v1/rooms/available          → List available rooms
GET    /api/v1/rooms/{id}              → Get room details
POST   /api/v1/rooms                   → Register new room
PUT    /api/v1/rooms/{id}              → Update room
PATCH  /api/v1/rooms/{id}/status       → Change room status
DELETE /api/v1/rooms/{id}              → Soft delete room

# Tenants
GET    /api/v1/tenants                 → List tenants (paginated)
GET    /api/v1/tenants/{id}            → Tenant details with current room
POST   /api/v1/tenants                 → Register tenant
PUT    /api/v1/tenants/{id}            → Update tenant

# Allocations
GET    /api/v1/allocations             → All active allocations
POST   /api/v1/allocations             → Assign tenant to room
PATCH  /api/v1/allocations/{id}/checkout → Checkout tenant

# Bills
GET    /api/v1/bills                   → List bills (filter by tenant, month, status)
GET    /api/v1/bills/{id}              → Bill details
POST   /api/v1/bills/generate          → Generate monthly bills
GET    /api/v1/bills/{id}/print        → Download PDF bill (QuestPDF)
GET    /api/v1/bills/pending-dues      → All unpaid bills with totals

# Payments
POST   /api/v1/payments                → Record a payment
GET    /api/v1/payments/tenant/{id}    → Payment history for tenant

# Dashboard
GET    /api/v1/dashboard               → Occupancy stats, revenue, overdue count

# System
GET    /health
GET    /health/ready
GET    /metrics
```

### Response Envelope Pattern
```json
{
  "success": true,
  "data": { ... },
  "message": null,
  "pagination": { "page": 1, "pageSize": 20, "totalCount": 150, "totalPages": 8 },
  "traceId": "00-abc123-def456-01"
}
```

---

## 🅰️ Angular 22+ Frontend

### Project Setup
```bash
ng new pg-management --standalone --routing --style=scss --ssr=false
# Use Angular Material 22+ for UI components
# Use Angular Signals for all state management (no NgRx needed)
# Use @angular/core inject() function over constructor injection
# Enable strict mode + strictTemplates
```

### Required Angular Packages
```
@angular/material@22+
@angular/cdk@22+
@angular/animations@22+
@ngrx/signals (for SignalStore if complex state needed)
@angular/common/http (HttpClient with provideHttpClient)
ngx-toastr or Angular Material Snackbar
chart.js + ng2-charts (dashboard charts)
ngx-skeleton-loader (loading states)
```

### Project Structure
```
src/
├── app/
│   ├── core/
│   │   ├── auth/
│   │   │   ├── auth.service.ts
│   │   │   ├── auth.guard.ts
│   │   │   └── auth.interceptor.ts
│   │   ├── interceptors/
│   │   │   ├── error.interceptor.ts        # Global error handling
│   │   │   └── loading.interceptor.ts
│   │   ├── services/
│   │   │   └── api.service.ts              # Base HTTP service
│   │   └── models/
│   ├── features/
│   │   ├── dashboard/
│   │   │   ├── dashboard.component.ts      # Standalone
│   │   │   └── dashboard.routes.ts
│   │   ├── rooms/
│   │   │   ├── room-list/
│   │   │   ├── room-form/
│   │   │   ├── room-availability/
│   │   │   ├── rooms.service.ts
│   │   │   ├── rooms.store.ts              # SignalStore
│   │   │   └── rooms.routes.ts
│   │   ├── tenants/
│   │   ├── bills/
│   │   │   ├── bill-list/
│   │   │   ├── bill-print/                 # Print preview
│   │   │   ├── pending-dues/
│   │   │   └── bills.service.ts
│   │   └── payments/
│   ├── shared/
│   │   ├── components/
│   │   │   ├── data-table/                 # Reusable paginated table
│   │   │   ├── confirmation-dialog/
│   │   │   ├── loading-spinner/
│   │   │   └── empty-state/
│   │   ├── pipes/
│   │   │   ├── currency-inr.pipe.ts
│   │   │   └── room-status.pipe.ts
│   │   └── validators/
│   ├── app.config.ts                       # provideRouter, provideHttpClient, etc.
│   ├── app.routes.ts
│   └── app.component.ts
└── environments/
    ├── environment.ts
    └── environment.production.ts
```

### Key Angular Implementation Requirements

#### Signals & State Management
```typescript
// Use Angular Signals for all reactive state
// Use computed() for derived state (e.g., available room count)
// Use effect() for side effects (e.g., auto-save drafts)
// Use toSignal() to convert Observables from HttpClient
// Use resource() API (Angular 19+) for async data loading with built-in loading/error states
```

#### HTTP & Error Handling
```typescript
// All API calls use typed HttpClient with generics
// Global error interceptor:
//   - 401 → redirect to login, clear token
//   - 403 → show "Access Denied" snackbar
//   - 429 → show "Rate limit exceeded, try again in Xs" with Retry-After
//   - 500 → show generic error + log to console/Sentry
// Use retry(2) with exponential backoff for transient 5xx errors
// Show correlation TraceId in error messages for support
```

#### Performance
```typescript
// Lazy load all feature routes
// Use OnPush change detection everywhere
// Virtualize long lists with @angular/cdk VirtualScroll
// Use trackBy on all *ngFor
// Defer non-critical UI with @defer blocks
// Preload adjacent routes with QuicklinkStrategy or PreloadAllModules (dev)
// Use Angular Image directive (NgOptimizedImage) for all images
```

#### Rooms Feature
```typescript
// Room list: filterable by status (Available / Occupied / Maintenance)
// Color-coded room cards: green=available, red=occupied, yellow=maintenance
// Real-time occupancy percentage gauge
// Click room → slide-over panel with tenant details
// "Register Room" form with reactive forms + FluentValidation-aligned validators
```

#### Bills & Dues Feature
```typescript
// Pending dues table: tenant, room, months overdue, total due, action buttons
// Generate bill: select month/year, preview charges before confirming
// Print bill: open formatted print dialog (CSS print media query OR download PDF via API)
// Bill history: paginated, filterable by paid/unpaid/partial
// Payment recording: inline form with payment mode selector
```

#### Dashboard
```typescript
// Summary cards: Total Rooms, Occupied, Available, Under Maintenance
// Revenue chart: monthly bar chart (last 12 months)
// Pending dues total (highlighted in red if > 0)
// Recent payments table
// Occupancy rate donut chart
```

---

## ⚙️ Configuration & Infrastructure

### appsettings.json Structure
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=PGManagementDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "SecretKey": "CHANGE_THIS_TO_A_256_BIT_SECRET_KEY",
    "Issuer": "PGManagementAPI",
    "Audience": "PGManagementClient",
    "ExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "Serilog": {
    "MinimumLevel": { "Default": "Information", "Override": { "Microsoft.EntityFrameworkCore": "Warning" } }
  },
  "FeatureManagement": {
    "BillPrintingEnabled": true,
    "OnlineDuesPayment": false,
    "AdvancedReporting": true,
    "TenantSelfPortal": false
  },
  "RateLimiting": {
    "AnonymousRequestsPerMinute": 30,
    "AuthenticatedRequestsPerMinute": 200
  },
  "OpenTelemetry": {
    "ServiceName": "PGManagement.API",
    "OtlpEndpoint": "http://localhost:4317"
  },
  "HealthChecks": {
    "MaxMemoryMB": 512,
    "MaxDiskUsagePercent": 90
  }
}
```

### Docker Compose (dev environment)
```yaml
# Include services for:
# - SQL Server 2022
# - Redis
# - Seq (log viewer)
# - Jaeger (distributed tracing)
# - Prometheus + Grafana (metrics)
# - Health Checks UI
```

---

## ✅ Code Quality & Best Practices

### Backend
- [ ] Clean Architecture with strict layer dependency rules
- [ ] CQRS with MediatR (Commands mutate, Queries read)
- [ ] Repository + Unit of Work pattern over DbContext
- [ ] FluentValidation pipeline behavior in MediatR
- [ ] All async methods end in `Async` and use `CancellationToken`
- [ ] No raw SQL except for optimized read queries (Dapper for complex reports)
- [ ] EF Core query splitting for N+1 prevention (`AsSplitQuery()`)
- [ ] Pagination on all list endpoints (`IQueryable` → `PagedList<T>`)
- [ ] Global query filters for soft-delete (`IsDeleted == false`)
- [ ] No business logic in controllers (thin controllers)
- [ ] All secrets in environment variables / User Secrets (never in source)
- [ ] Nullable reference types enabled
- [ ] Analyzers: Roslynator, SonarAnalyzer

### Frontend
- [ ] Strict TypeScript + no `any` types
- [ ] All components standalone (no NgModules)
- [ ] Reactive forms with typed `FormGroup<T>`
- [ ] Environment-specific API base URLs
- [ ] Tailwind CSS utility classes OR Angular Material theming (pick one)
- [ ] Accessibility: ARIA labels, keyboard navigation, color contrast
- [ ] Unit tests: Jasmine + Angular Testing Library for components
- [ ] E2E tests: Playwright

### Security
- [ ] JWT with short expiry + refresh token rotation
- [ ] CORS restricted to known origins
- [ ] HTTPS enforced (HSTS header)
- [ ] Input sanitization on all user inputs
- [ ] SQL injection prevention (parameterized queries via EF Core)
- [ ] Sensitive data never logged (email, phone, ID proof numbers)
- [ ] Rate limiting on auth endpoints (stricter: 5 req/min for login)
- [ ] Security headers: CSP, X-Frame-Options, X-Content-Type-Options

---

## 📋 Implementation Order

```
Phase 1 — Infrastructure & Scaffolding
  1. Set up solution structure, NuGet packages
  2. Configure EF Core, run migrations, seed data
  3. Configure Serilog, OpenTelemetry, Health Checks
  4. Configure JWT auth, rate limiting
  5. Set up global exception handling middleware

Phase 2 — Core Domain (Backend)
  6. Rooms CRUD API + availability endpoint
  7. Tenants CRUD API
  8. Room allocations (check-in / check-out)
  9. Bill generation + pending dues query
  10. Payments recording
  11. Bill PDF export (QuestPDF)
  12. Dashboard aggregation query

Phase 3 — Angular Frontend
  13. Project setup, routing, layout shell, auth
  14. Dashboard page
  15. Rooms feature (list, availability, register)
  16. Tenants feature
  17. Bills & dues feature (list, generate, print)
  18. Payments feature

Phase 4 — Hardening
  19. Write unit + integration tests
  20. Add Redis caching to hot endpoints
  21. Performance profiling + query optimization
  22. Security audit
  23. Docker Compose + CI/CD pipeline (GitHub Actions)
```

---

## 🧪 Sample AI Prompt Starters (use these for each phase)

> **For database migration:**  
> "Generate EF Core entity classes and `IEntityTypeConfiguration` files for Rooms, Tenants, RoomAllocations, Bills, and Payments following the SQL schema above. Use clean architecture conventions, include global query filters for soft-delete, and add appropriate indexes."

> **For a controller:**  
> "Create a slim ASP.NET Core 10 controller for Rooms with API versioning (v1). Use MediatR to dispatch commands and queries. Apply rate limiting policy, feature gate, and return PagedList wrapped in the standard response envelope. Include Swagger/OpenAPI annotations."

> **For the Angular rooms feature:**  
> "Create an Angular 22 standalone RoomsListComponent using Signals and the resource() API. Display rooms in a color-coded card grid (green/red/yellow by status). Add a filter bar for status. Use Angular Material cards. Use OnPush change detection. Connect to the rooms API service."

> **For bill printing:**  
> "Create a QuestPDF document class that generates a professional bill PDF for a PG tenant. Include: PG name/logo header, tenant details, room info, bill month, itemized charges table (rent, electricity, water, other), total, payment history, and footer with contact info."

> **For health checks:**  
> "Configure ASP.NET Core health checks with SQL Server, Redis, memory, and disk space checks. Register /health, /health/ready, /health/live endpoints. Add the HealthChecks UI dashboard at /health/ui. Return JSON with per-component status and duration."
```

---

*Generated prompt for Paying Guest Management System · Angular 22+ · .NET 10 · SQL Server*  
*Save and use sections of this prompt individually with your AI coding assistant for best results.*
