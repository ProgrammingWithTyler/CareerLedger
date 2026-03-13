# CareerLedger — Backend API

**Career Management System — OpportunityTracker MVP**

CareerLedger is a modular backend API system built for engineers managing an active job search. The OpportunityTracker module tracks job applications, interview pipelines, and hiring lifecycle events with complete auditability.

## Modules

### OpportunityTracker — Current Focus (MVP)

Track job applications and hiring pipeline progress with immutable event history.

**MVP Features:**

- Job application tracking with lifecycle events
- State machine validation (Submitted → InReview → PhoneScreen → Interview → Offer)
- Analytics: response rate, interview rate, offer rate, time-to-milestone
- Complete audit trail via immutable events

### ImpactLog — Post-MVP

Log completed engineering work for resume reconstruction. Planned after OpportunityTracker is deployed and in use.

---

## Architecture

**Clean Architecture — .NET 10 + PostgreSQL 16**

```
CareerLedger.Backend/
├── src/
│   ├── CareerLedger.Api/              # ASP.NET Core Web API — controllers, middleware, Swagger
│   ├── CareerLedger.Application/      # Use cases, DTOs, services, validators, AutoMapper
│   ├── CareerLedger.Domain/           # Entities, enums, lifecycle rules (ZERO external dependencies)
│   └── CareerLedger.Infrastructure/   # EF Core, repositories, migrations, PostgreSQL
└── tests/
    ├── CareerLedger.UnitTests/        # Domain + Application layer tests (xUnit + Moq)
    ├── CareerLedger.IntegrationTests/ # Full API tests (Testcontainers + WebApplicationFactory)
    └── CareerLedger.TestUtilities/    # Shared test data builders
```

**Key Principles:**

- Domain layer has zero external NuGet dependencies
- Application is the aggregate root for ApplicationEvents
- PostgreSQL with snake_case naming conventions
- JWT authentication (single-user for MVP)
- RESTful API with Swagger documentation
- Events are immutable — append-only audit trail
- `CurrentStatus` derived from latest event, never stored

---

## Domain Model

### Application (Aggregate Root)

```
Id: Guid
AccountId: Guid
CompanyName: string (1–255 chars)
JobTitle: string (1–255 chars)
JobUrl: string? (optional, max 2048 chars)
CreatedAt: DateTime (UTC)
CurrentStatus: EventType (computed from latest event — NOT stored)
Events: ICollection<ApplicationEvent>
```

### ApplicationEvent (Immutable)

```
Id: Guid
ApplicationId: Guid
AccountId: Guid (denormalized for analytics)
EventType: enum (10 states)
OccurredAt: DateTime (UTC — when it happened)
Notes: string? (optional, max 5000 chars)
CreatedAt: DateTime (UTC — when logged)
```

**Business Rules:**

- Events are create-only — event type and date cannot be edited
- Valid transitions enforced by `ApplicationLifecycleService`
- `CurrentStatus` = most recent event's `EventType`
- Cannot transition from terminal states

### EventType Lifecycle

```
Flow: Submitted → InReview → PhoneScreen → TechnicalInterview → OnsiteInterview → OfferReceived

Terminal: OfferAccepted | OfferDeclined | Rejected | Withdrawn

Rules:
- First event must be Submitted
- No backward transitions
- Rejected and Withdrawn valid from any non-terminal state
- TechnicalInterview → OfferReceived valid (onsite is optional)
- No further events after any terminal state
```

### Example Application Journey

**TechCorp — Senior Software Engineer:**

1. **Submitted** (2026-02-01) — "Applied via LinkedIn"
2. **InReview** (2026-02-05) — "Recruiter confirmed receipt"
3. **PhoneScreen** (2026-02-08) — "30-minute call with Sarah (HR)"
4. **TechnicalInterview** (2026-02-12) — "Pair programming session with team lead"
5. **OnsiteInterview** (2026-02-19) — "Full-day onsite: 5 interviews"
6. **OfferReceived** (2026-02-22) — "Offer: $150k base, $30k equity"
7. **OfferAccepted** (2026-02-24) — "Start date: March 15"

**CurrentStatus:** `OfferAccepted` (derived from latest event — 23 days submission to offer)

---

## Quick Start (Windows 11 + PowerShell)

### Prerequisites

**Required:**

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PostgreSQL 16](https://www.postgresql.org/download/windows/) or Docker
- [Git](https://git-scm.com/downloads)

**Recommended:**

- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- [pgAdmin 4](https://www.pgadmin.org/) (included with PostgreSQL installer)
- [Postman](https://www.postman.com/downloads/) or use Swagger UI

### Installation

**1. Clone Repository**

```powershell
cd C:\Users\$env:USERNAME\Projects
git clone https://github.com/yourusername/CareerLedger.Backend.git
cd CareerLedger.Backend
```

**2. Set Up PostgreSQL**

Option A — Local installation (recommended):

- Download PostgreSQL 16 installer for Windows
- Run installer, set a postgres password (remember it)
- Default port: 5432, pgAdmin 4 included

Option B — Docker:

```powershell
docker-compose up -d
```

**3. Create Database**

```powershell
psql -U postgres
# Enter your password

# At psql prompt:
CREATE DATABASE careerledger;
\q
```

Or use pgAdmin 4: right-click Databases → Create → Database → name it `careerledger`.

**4. Configure Connection String**

Create `src/CareerLedger.Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=careerledger;Username=postgres;Password=YOUR_PASSWORD_HERE"
  },
  "JwtSettings": {
    "SecretKey": "your-development-secret-key-must-be-at-least-32-characters",
    "Issuer": "CareerLedger",
    "Audience": "CareerLedgerAPI",
    "ExpirationMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

> **Important:** This file is in `.gitignore` — never commit it.

**5. Restore Dependencies**

```powershell
dotnet restore
```

**6. Apply Database Migrations**

```powershell
cd src\CareerLedger.Api

dotnet ef database update `
  --project ..\CareerLedger.Infrastructure `
  --startup-project .
```

**7. Run the Application**

```powershell
dotnet run
# Or with hot reload:
dotnet watch run
```

**8. Open Swagger UI**

Navigate to: `https://localhost:5001/swagger`

---

## Authentication

CareerLedger is a **single-user system** for MVP. There is no registration endpoint.

### Default Dev Account (Seed Data)

When running in Development, the database seeder (CL-021) creates:

- **Email:** `test@example.com`
- **Password:** `Test123!`

### Manual Account Creation

```sql
INSERT INTO accounts (email, password_hash, is_active)
VALUES (
  'your.email@example.com',
  '$2a$11$your_bcrypt_hash_here',
  true
);
```

Use BCrypt to generate the hash — cost factor 11.

### Login

```powershell
$body = @{
    email    = "test@example.com"
    password = "Test123!"
} | ConvertTo-Json

$response = Invoke-RestMethod `
    -Uri "https://localhost:5001/api/auth/login" `
    -Method Post `
    -Body $body `
    -ContentType "application/json" `
    -SkipCertificateCheck

$token = $response.token
```

### Using the Token

Add to every subsequent request:

```
Authorization: Bearer YOUR_TOKEN_HERE
```

In Swagger UI: click **Authorize** → enter `Bearer YOUR_TOKEN_HERE` → click **Authorize**.

---

## API Endpoints

### Authentication

```
POST /api/auth/login
  Body:    { email, password }
  → 200:   { token, expiresAt, user: { id, email } }
  → 401:   { error: "Unauthorized", message: "Invalid email or password" }
```

### Applications

```
POST   /api/applications
  Body:    { companyName, jobTitle, jobUrl?, submittedAt, notes? }
  → 201:   Application with initial Submitted event + Location header
  → 400:   Validation errors (field-level)

GET    /api/applications?status=InReview&company=Tech&page=1&pageSize=20&sortBy=createdAt&sortOrder=desc
  → 200:   { items[], page, pageSize, totalCount, totalPages }

GET    /api/applications/{id}
  → 200:   Application with eventCount and currentStatus
  → 404

PUT    /api/applications/{id}
  Body:    { companyName, jobTitle, jobUrl? }
  → 200:   Updated application
  → 400 | 404

DELETE /api/applications/{id}
  → 204:   Soft delete (creates Withdrawn event)
  → 400:   Already in terminal state
```

### Events

```
POST   /api/applications/{applicationId}/events
  Body:    { eventType, occurredAt, notes? }
  → 201:   Created event
  → 400:   Invalid transition (message includes current and proposed state)

GET    /api/applications/{applicationId}/events
  → 200:   Chronological event list

PATCH  /api/applications/{applicationId}/events/{eventId}
  Body:    { notes }
  → 200:   Updated event (notes only — eventType and occurredAt are immutable)

DELETE /api/applications/{applicationId}/events/{eventId}
  → 204:   Event deleted
  → 400:   Cannot delete initial Submitted event
```

### Analytics

```
GET /api/analytics/overview?from=2026-01-01&to=2026-03-01
  → 200:   { totalApplications, activeApplications, responseRate,
             interviewRate, offerRate, avgDaysToResponse,
             avgDaysToInterview, avgDaysToOffer, computedAt }

GET /api/analytics/funnel
  → 200:   [{ stage, count, percentage }] for each lifecycle stage

GET /api/analytics/status-distribution
  → 200:   Breakdown of applications by current status
```

### Health

```
GET /health  (public — no auth required)
  → 200:   { status: "Healthy", checks: { database: "Healthy" } }
  → 503:   { status: "Unhealthy", checks: { database: "Unhealthy" } }
```

---

## Testing

### Run All Tests

```powershell
dotnet test
```

### Run by Project

```powershell
dotnet test tests\CareerLedger.UnitTests
dotnet test tests\CareerLedger.IntegrationTests
```

### Generate Coverage Report

```powershell
dotnet test /p:CollectCoverage=true `
            /p:CoverletOutputFormat=opencover `
            /p:CoverletOutput=./coverage/
```

**Test strategy:**

- **Unit Tests (CL-019):** Domain entities, `ApplicationLifecycleService`, application services, FluentValidation validators. Target: 95%+ Domain layer, 90%+ Application layer. Framework: xUnit + Moq + FluentAssertions.
- **Integration Tests (CL-020):** Full API request-response cycle with real PostgreSQL via Testcontainers. All endpoints, authentication, authorization, lifecycle transitions, analytics. Framework: xUnit + WebApplicationFactory + Testcontainers.

See `TESTING-STRATEGIES.md` for full specification.

---

## Usage Examples

### Creating an Application

```powershell
$headers = @{ Authorization = "Bearer $token" }

$body = @{
    companyName = "TechCorp"
    jobTitle    = "Senior Software Engineer"
    jobUrl      = "https://techcorp.com/careers/senior-engineer"
    submittedAt = "2026-02-01T09:00:00Z"
    notes       = "Applied through LinkedIn. Recruiter is Sarah Johnson."
} | ConvertTo-Json

$app = Invoke-RestMethod `
    -Uri "https://localhost:5001/api/applications" `
    -Method Post `
    -Headers $headers `
    -Body $body `
    -ContentType "application/json" `
    -SkipCertificateCheck

# Response: { id: "...", companyName: "TechCorp", currentStatus: "Submitted", ... }
$applicationId = $app.id
```

### Adding Lifecycle Events

```powershell
# Recruiter responded
$event1 = @{
    eventType  = "InReview"
    occurredAt = "2026-02-05T10:30:00Z"
    notes      = "Sarah confirmed receipt, scheduling phone screen"
} | ConvertTo-Json

Invoke-RestMethod `
    -Uri "https://localhost:5001/api/applications/$applicationId/events" `
    -Method Post -Headers $headers -Body $event1 -ContentType "application/json" -SkipCertificateCheck

# Phone screen completed
$event2 = @{
    eventType  = "PhoneScreen"
    occurredAt = "2026-02-08T14:00:00Z"
    notes      = "30-minute call. Discussed team structure. Next: technical interview."
} | ConvertTo-Json

Invoke-RestMethod `
    -Uri "https://localhost:5001/api/applications/$applicationId/events" `
    -Method Post -Headers $headers -Body $event2 -ContentType "application/json" -SkipCertificateCheck

# CurrentStatus is now "PhoneScreen" (derived from latest event)
```

### Invalid Transition Example

```powershell
# Trying to jump from Submitted directly to OfferReceived
$invalidEvent = @{ eventType = "OfferReceived"; occurredAt = "2026-02-06T09:00:00Z" } | ConvertTo-Json

# Returns 400:
# { error: "ValidationError", message: "Invalid transition from Submitted to OfferReceived.
#   Valid next states: InReview, Rejected, Withdrawn" }
```

### Analytics

```powershell
$overview = Invoke-RestMethod `
    -Uri "https://localhost:5001/api/analytics/overview" `
    -Headers $headers -SkipCertificateCheck

# {
#   totalApplications: 47,
#   responseRate: 68.5,      ← 68.5% of companies responded
#   interviewRate: 42.3,     ← 42.3% reached interview stage
#   offerRate: 12.8,         ← 12.8% resulted in offers
#   avgDaysToResponse: 4.2   ← companies respond in ~4 days
# }
```

**Reading your numbers:**

- `responseRate < 50%` → Resume or application quality needs work
- `interviewRate` low but `responseRate` high → Getting noticed, failing at screening
- `avgDaysToResponse > 7` → Slow companies, apply more broadly
- Track `offerRate` over time to measure improvement

---

## Database Schema

```sql
CREATE TABLE accounts (
    id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email         VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(512) NOT NULL,
    is_active     BOOLEAN NOT NULL DEFAULT true,
    created_at    TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE applications (
    id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    account_id   UUID NOT NULL REFERENCES accounts(id) ON DELETE CASCADE,
    company_name VARCHAR(255) NOT NULL,
    job_title    VARCHAR(255) NOT NULL,
    job_url      VARCHAR(2048),
    created_at   TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_applications_account_id
    ON applications(account_id) INCLUDE (company_name, job_title, created_at);
CREATE INDEX idx_applications_created_at
    ON applications(created_at DESC);

CREATE TABLE application_events (
    id             UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    application_id UUID NOT NULL REFERENCES applications(id) ON DELETE CASCADE,
    account_id     UUID NOT NULL REFERENCES accounts(id) ON DELETE NO ACTION,
    event_type     VARCHAR(50) NOT NULL,
    occurred_at    TIMESTAMP NOT NULL,
    notes          TEXT,
    created_at     TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT chk_event_type CHECK (event_type IN (
        'Submitted', 'InReview', 'PhoneScreen', 'TechnicalInterview',
        'OnsiteInterview', 'OfferReceived', 'OfferAccepted',
        'OfferDeclined', 'Rejected', 'Withdrawn'
    ))
);

-- Covering index for CurrentStatus derivation (<5ms)
CREATE INDEX idx_application_events_app_occurred
    ON application_events(application_id, occurred_at DESC, created_at DESC)
    INCLUDE (event_type);

-- Analytics queries
CREATE INDEX idx_application_events_account_occurred
    ON application_events(account_id, occurred_at DESC)
    INCLUDE (application_id, event_type);
```

**Key design decisions:**

- `CurrentStatus` is never stored — always derived from latest event via covering index
- Events are immutable — INSERT only, no UPDATE on event type or date
- Cascade delete: account → applications → events
- `ON DELETE NO ACTION` on events → accounts FK prevents cascade conflict

---

## Development Workflow

### Daily Startup

```powershell
# Start PostgreSQL if not auto-starting
Start-Service postgresql-x64-16

# Navigate and pull
cd C:\Users\$env:USERNAME\Projects\CareerLedger.Backend
git pull

# Run with hot reload
cd src\CareerLedger.Api
dotnet watch run
```

### Adding a Migration

```powershell
cd src\CareerLedger.Api

dotnet ef migrations add MigrationName `
  --project ..\CareerLedger.Infrastructure `
  --startup-project .

dotnet ef database update `
  --project ..\CareerLedger.Infrastructure `
  --startup-project .
```

### Resetting Database

```powershell
dotnet ef database drop --project ..\CareerLedger.Infrastructure --force
dotnet ef database update --project ..\CareerLedger.Infrastructure
```

---

## Troubleshooting

**"Connection refused" to PostgreSQL**

```powershell
Get-Service postgresql-x64-16   # Check status
Start-Service postgresql-x64-16 # Start if stopped
```

**"Login failed for user 'postgres'"**

```powershell
psql -U postgres
ALTER USER postgres WITH PASSWORD 'NewPassword123!';
\q
# Then update appsettings.Development.json
```

**"No such table" errors**

```powershell
cd src\CareerLedger.Api
dotnet ef database update --project ..\CareerLedger.Infrastructure
```

**Port 5001 already in use**

```powershell
Get-Process -Name dotnet | Stop-Process -Force
```

---

## Project Status

**Version:** 1.0 (OpportunityTracker MVP)
**Status:** Active Development — Phase 1 completing
**Target:** Working, deployed job tracker in 5–6 weeks

### Phase 1: Foundation & Domain (Weeks 1–2)

| Ticket | Description | Status |
|--------|-------------|--------|
| CL-001 | Solution structure (Clean Architecture) | ✅ Completed |
| CL-002 | NuGet packages | ✅ Completed |
| CL-003 | Domain entities | ✅ Completed |
| CL-004 | ApplicationLifecycleService | 🔄 In Progress |
| CL-005 | EF Core DbContext + entity configurations | Todo |
| CL-006 | Initial database migration | Todo |

### Phase 2: Infrastructure & Application (Weeks 3–4)

| Ticket | Description | Status |
|--------|-------------|--------|
| CL-007 | Repository interfaces and implementations | Todo |
| CL-008 | DTOs and AutoMapper profiles | Todo |
| CL-009 | FluentValidation validators | Todo |
| CL-010 | Application services | Todo |

### Phase 3: API & Auth (Weeks 4–5)

| Ticket | Description | Status |
|--------|-------------|--------|
| CL-011 | JWT authentication | Todo |
| CL-012 | ApplicationsController | Todo |
| CL-013 | EventsController | Todo |
| CL-015 | Swagger/OpenAPI documentation | Todo |
| CL-016 | Global error handling middleware | Todo |

**🎉 Milestone: Working API — can create and track applications via HTTP**

### Phase 4: Analytics & Enhancement (Week 5–6)

| Ticket | Description | Status |
|--------|-------------|--------|
| CL-014 | Analytics service and controller | Todo |
| CL-017 | Health check endpoint | Todo |
| CL-018 | CORS policy | Todo |

### Phase 5: Testing & Deployment (Weeks 6–8)

| Ticket | Description | Status |
|--------|-------------|--------|
| CL-019 | Comprehensive unit tests (95%+ Domain) | Todo |
| CL-020 | Integration tests with Testcontainers | Todo |
| CL-021 | Database seed data | Todo |
| CL-022 | README, deployment guide, API reference | Todo |
| CL-023 | Deploy to Digital Ocean App Platform | Todo |
| CL-024 | GitHub Actions CI/CD pipeline | Todo |

**🚀 Milestone: Deployed, tested, production-ready**

---

## Architecture Decisions

### Why PostgreSQL over SQL Server?

| Aspect | PostgreSQL | SQL Server |
|--------|-----------|------------|
| Cost | Free | ~$1,400/core or $30–50/month cloud |
| Managed hosting | $15/month (Digital Ocean) | $30–50/month (Azure SQL) |
| Platform | Windows, Mac, Linux, Docker | Primarily Windows |
| Decision | **Use this** | Overkill for this use case |

### Why Digital Ocean over Azure?

| Component | Digital Ocean | Azure |
|-----------|--------------|-------|
| PostgreSQL | $15/month | $12/month |
| Backend API | $5/month | $13/month |
| Frontend | $0/month | $0/month |
| **Total** | **$20/month** | **$25/month** |

$60/year savings. Simpler dashboard for solo developer. Predictable pricing.

### Why Immutable Events?

- Complete audit trail — see exactly when each status changed and why
- Cannot lose history — events never hard-deleted
- `CurrentStatus` derived from events — single source of truth, no duplicate state
- Simplified concurrency — append-only eliminates update conflicts
- Easy timeline visualizations for the frontend

### Why Clean Architecture?

- Domain layer is fully testable with no infrastructure dependencies
- Can swap database or ORM without touching business logic
- Application and Domain layers are reusable if a CLI or worker is added later
- Clear boundary for where each type of logic lives

### Why xUnit over MSTest?

Better community support, idiomatic with FluentAssertions, more flexible test lifecycle. All tests use `[Fact]` / `[Theory]` — not `[TestMethod]`.

### Why Testcontainers over In-Memory Database?

In-memory EF Core databases don't enforce PostgreSQL constraints, check constraints, or cover all query behaviors. Testcontainers runs a real PostgreSQL 16 container — same version as production. Catches real SQL issues that mocks miss.

### Technology Stack Summary

| Layer | Technology |
|-------|-----------|
| Backend API | ASP.NET Core Web API (.NET 10) |
| Database | PostgreSQL 16 |
| ORM | Entity Framework Core + Npgsql |
| Validation | FluentValidation |
| Mapping | AutoMapper |
| Authentication | JWT Bearer (BCrypt passwords) |
| Unit testing | xUnit + Moq + FluentAssertions |
| Integration testing | xUnit + Testcontainers + WebApplicationFactory |
| CI/CD | GitHub Actions |
| Hosting | Digital Ocean App Platform ($20/month) |
| Frontend (separate repo) | Angular 20+ |

---

## Additional Documentation

- [Architecture.md](docs/Architecture.md) — Full system design, data schema, index strategy, future evolution
- [QUICK-REFERENCE-ARCHITECTURE.md](docs/QUICK-REFERENCE-ARCHITECTURE.md) — Implementation guide and decision reference
- [TESTING-STRATEGIES.md](docs/TESTING-STRATEGIES.md) — Complete unit and integration test specifications
- [tickets-backend.json](docs/tickets-backend.json) — All 24 tickets with full acceptance criteria

---

## Contributing

Personal project — suggestions welcome via GitHub Issues.

Include: error messages, steps to reproduce, environment (Windows version, .NET version, PostgreSQL version).

---

## License

BSD License — see [LICENSE](LICENSE) for details.

---

**Developer:** Tyler Johnson
**GitHub:** [@programmingwithtyler](https://github.com/programmingwithtyler)
