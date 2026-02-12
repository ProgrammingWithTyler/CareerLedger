# CareerLedger - Backend API

**OpportunityTracker (Job Application Tracking) + ImpactLog (Work Logging)**

## Quick Start

### Prerequisites
- .NET 8 SDK
- PostgreSQL 16
- Git

### Setup

```powershell
# Restore packages
dotnet restore

# Build
dotnet build

# Run API
cd src/CareerLedger.Api
dotnet run
```

API will be available at: https://localhost:5001

### Project Structure

```
Backend/
├── src/
│   ├── CareerLedger.Api/              # ASP.NET Core Web API
│   ├── CareerLedger.Application/      # Use cases, DTOs, services
│   ├── CareerLedger.Domain/           # Entities, enums (zero dependencies)
│   └── CareerLedger.Infrastructure/   # EF Core, repositories, PostgreSQL
└── tests/
    ├── CareerLedger.UnitTests/
    ├── CareerLedger.IntegrationTests/
    └── CareerLedger.TestUtilities/
```

## Architecture

**Clean Architecture** with dependency inversion:
- Domain layer has ZERO external dependencies
- Application layer depends only on Domain
- Infrastructure implements interfaces from Application
- API orchestrates everything

## Tech Stack

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL 16 (Npgsql)
- xUnit (testing)
- AutoMapper
- FluentValidation
- JWT Authentication

## Next Steps

1. **CL-002:** Install NuGet packages
2. **CL-003:** Create domain entities
3. See tickets for full implementation plan

## Documentation

- [QUICK-REFERENCE-ARCHITECTURE.md](QUICK-REFERENCE-ARCHITECTURE.md)
- [TESTING-STRATEGIES.md](TESTING-STRATEGIES.md)
- [API-Contracts.md](API-Contracts.md) (coming soon)
