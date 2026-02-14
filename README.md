# CareerLedger

**Modular Career Management System**

## Structure

```code
CareerLedger/                    ← Monorepo root
├── Backend/                     ← .NET 10 API (OpportunityTracker + ImpactLog)
│   ├── src/
│   ├── tests/
│   └── CareerLedger.sln
└── Frontend/                    ← (Coming soon) Blazor Unified
```

## Modules

### 1. OpportunityTracker (MVP - In Development)

Job application tracking with lifecycle management, analytics, and interview tracking.

### 2. ImpactLog (Post-MVP)

Engineering work logging for resume reconstruction.

## Getting Started

### Backend

```powershell
cd Backend
dotnet restore
dotnet build
```

See [Backend/README.md](Backend/README.md) for full setup instructions.

### Frontend

Coming after backend MVP is complete.

## Tech Stack

**Backend:**

- .NET 10
- PostgreSQL 16
- EF Core
- Clean Architecture

**Frontend (Planned):**

- Blazor Unified
- Custom Design System

## Documentation

- [Backend README](Backend/README.md)
- [Quick Reference Architecture](docs/QUICK-REFERENCE-ARCHITECTURE.md)
- [Testing Strategies](docs/TESTING-STRATEGIES.md)
