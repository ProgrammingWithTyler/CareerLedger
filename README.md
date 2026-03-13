# CareerLedger

**Modular Career Management System for Engineers**

## Structure

```text
CareerLedger/                    ← Monorepo root
├── Backend/                     ← .NET 10 Web API (OpportunityTracker + ImpactLog)
│   ├── src/
│   ├── tests/
│   └── CareerLedger.sln
└── Frontend/                    ← Angular 20+ (post-backend MVP)
```

## Modules

### 1. OpportunityTracker — MVP (In Active Development)

Job application tracking with immutable event-based lifecycle management, transition validation, and pipeline analytics (response rate, interview rate, time-to-offer).

### 2. ImpactLog — Post-MVP

Engineering work logging for resume reconstruction — track features, bugs, and incidents with outcome metrics so "what did I do last year?" is never a question again.

## Getting Started

### Backend

```powershell
cd Backend
dotnet restore
dotnet build
```

See [Backend/README.md](Backend/README.md) for full setup: PostgreSQL, migrations, JWT config, seed data, and Swagger UI.

### Frontend

Planned after backend MVP is deployed. See roadmap below.

## Tech Stack

**Backend:**

- .NET 10 (ASP.NET Core Web API)
- PostgreSQL 16 — snake_case naming, immutable event schema
- Entity Framework Core + Npgsql
- FluentValidation + AutoMapper
- JWT authentication (single-user MVP)
- xUnit + Moq + FluentAssertions + Testcontainers
- Deployed to Digital Ocean App Platform ($20/month)

**Frontend (Planned):**

- Angular 20+
- Custom design system

## Roadmap

| Phase | Scope | Status |
|-------|-------|--------|
| 1 — Foundation | Domain entities, lifecycle service, EF Core, migrations | 🔄 In Progress |
| 2 — Application Layer | Repositories, services, DTOs, validators | Todo |
| 3 — API & Auth | Controllers, JWT, Swagger, error handling | Todo |
| 4 — Analytics | Pipeline analytics, health check, CORS | Todo |
| 5 — Testing & Deploy | Unit tests, integration tests, CI/CD, Digital Ocean | Todo |
| Frontend | Angular 20+ UI — dashboard, kanban, analytics | Todo |
| ImpactLog | Work logging module — post-MVP | Todo |

## Documentation

- [Backend README](Backend/README.md) — Setup, API endpoints, usage examples
- [Architecture](Backend/docs/Architecture.md) — System design, schema, decisions
- [Quick Reference](Backend/docs/QUICK-REFERENCE-ARCHITECTURE.md) — Implementation guide
- [Testing Strategies](Backend/docs/TESTING-STRATEGIES.md) — Unit and integration test specs
