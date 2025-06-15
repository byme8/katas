# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run Commands

### Backend (API)
- Build: `dotnet build Pagination.sln`
- Run with Aspire (recommended): `dotnet run --project Pagination.Aspire/Pagination.Aspire.csproj`
- Run API directly: `dotnet run --project Pagination/Pagination.csproj`
- Test endpoints: Use `Pagination/Pagination.http` file with REST Client

### Frontend (SPA)
- Install dependencies: `cd Pagination.SPA && bun install`
- Run development server: `bun start` (serves at http://localhost:4200)
- Build for production: `bun run build`
- Run tests: `bun test`

## Database Commands
- Add migration: `dotnet ef migrations add <MigrationName> --project Pagination/Pagination.csproj`
- Update database: `dotnet ef database update --project Pagination/Pagination.csproj`
- Remove last migration: `dotnet ef migrations remove --project Pagination/Pagination.csproj`

## Prerequisites
- Install .NET Aspire workload: `dotnet workload install aspire`
- Install EF Core CLI tools: `dotnet tool install --global dotnet-ef`
- Docker Desktop required for PostgreSQL when using Aspire
- Install Bun for SPA development: https://bun.sh

## Database Options
### Option 1: Aspire-managed PostgreSQL (default)
- Database starts/stops with Aspire
- Data persists in Docker volume
- Easiest for development

### Option 2: External PostgreSQL with Docker Compose
- Database runs independently
- Survives Aspire restarts
- Run: `docker-compose up -d` to start
- Stop: `docker-compose down` (add `-v` to remove data)

### Option 3: External PostgreSQL instance
- Use existing PostgreSQL server
- Update connection string in Aspire Program.cs or appsettings.json

## Architecture Overview
The solution consists of four projects:
- **Pagination.Aspire**: Orchestrates PostgreSQL container and API, provides dashboard at http://localhost:9000 and pgAdmin at http://localhost:8080
- **Pagination**: Main API with offset-based pagination endpoints at http://localhost:10000
- **Pagination.ServiceDefaults**: Shared Aspire services (telemetry, health checks, service discovery)
- **Pagination.SPA**: Angular 20 frontend application with pagination UI at http://localhost:4200 (uses signals for state management)

## Key Patterns & Practices
- Minimal APIs with extension methods for handler registration (e.g., `MapUsers()`)
- Entity Framework Core with PostgreSQL for data access (`PaginationDbContext`)
- Hosted service for automatic database seeding and migrations (`DatabaseSeedingService`)
- Generic pagination models with sorting/ordering support
- Soft delete pattern with IsDeleted/DeletedAt fields and query filters
- EF Core indexes for performance optimization
- Validation extensions for pagination requests
- Automatic timestamp updates on SaveChanges

## API Endpoints
All API endpoints are prefixed with `/api`:
- `GET /api/companies` - Returns all companies
- `POST /api/companies/offset` - Offset pagination for all companies
- `GET /api/companies/{companyId}/users` - Returns all users for a company (companyId is a number)
- `POST /api/companies/{companyId}/users/offset` - Offset pagination for company users
- `POST /api/users/offset` - Offset pagination for all users
- `GET /api/users/all` - Returns all users without pagination

## Data Model
- **Company**: Id (long), Name, Email, CreatedAt, UpdatedAt, IsDeleted, DeletedAt
- **User**: Id (long), CompanyId (long), Name, Email, CreatedAt, UpdatedAt, IsDeleted, DeletedAt
- Database is automatically seeded with:
  - 1 special "Small Company" (ID: 1) with 15 users
  - 100,000 regular companies (IDs: 2-100,001) with random 25-5,000 users each (~250M total users on average)

## Development Guidelines
- Use async/await for all database operations
- Return appropriate HTTP status codes (400 for validation, 404 for not found)
- Implement early returns for validation/guard clauses
- Use dependency injection for all services
- Follow minimal API patterns with static handler classes
- Enable nullable reference types
- Use primary constructors where appropriate