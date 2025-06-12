# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run Commands
- Build: `dotnet build Pagination.sln`
- Run with Aspire (recommended): `dotnet run --project Pagination.Aspire/Pagination.Aspire.csproj`
- Run API directly: `dotnet run --project Pagination/Pagination.csproj`
- Test endpoints: Use `Pagination/Pagination.http` file with REST Client

## Database Commands
- Add migration: `dotnet ef migrations add <MigrationName> --project Pagination/Pagination.csproj`
- Update database: `dotnet ef database update --project Pagination/Pagination.csproj`
- Remove last migration: `dotnet ef migrations remove --project Pagination/Pagination.csproj`

## Prerequisites
- Install .NET Aspire workload: `dotnet workload install aspire`
- Install EF Core CLI tools: `dotnet tool install --global dotnet-ef`
- Docker Desktop required for PostgreSQL when using Aspire

## Architecture Overview
The solution consists of three projects:
- **Pagination.Aspire**: Orchestrates PostgreSQL container and API, provides dashboard at http://localhost:9000 and pgAdmin at http://localhost:8080
- **Pagination**: Main API with offset-based pagination endpoints at http://localhost:10000
- **Pagination.ServiceDefaults**: Shared Aspire services (telemetry, health checks, service discovery)

## Key Patterns & Practices
- Minimal APIs with extension methods for handler registration (e.g., `MapComments()`)
- Entity Framework Core with PostgreSQL for data access (`PaginationDbContext`)
- Hosted service for automatic database seeding and migrations (`DatabaseSeedingService`)
- Generic pagination models with sorting/ordering support
- Soft delete pattern with IsDeleted/DeletedAt fields and query filters
- EF Core indexes for performance optimization
- Validation extensions for pagination requests
- Automatic timestamp updates on SaveChanges

## API Endpoints
- `GET /users` - Returns all users
- `GET /users/{userId}/comments` - Returns all comments for a user (userId is a GUID)
- `POST /users/{userId}/comments/offset` - Offset pagination for user comments
- `POST /comments/offset` - Offset pagination for all comments

## Data Model
- **User**: Id, Name, Email, CreatedAt, UpdatedAt, IsDeleted, DeletedAt
- **Comment**: Id, UserId, Message, CreatedAt, UpdatedAt, IsDeleted, DeletedAt
- Database is automatically seeded with 100,000 users and 1,000 comments each (100M total)

## Development Guidelines
- Use async/await for all database operations
- Return appropriate HTTP status codes (400 for validation, 404 for not found)
- Implement early returns for validation/guard clauses
- Use dependency injection for all services
- Follow minimal API patterns with static handler classes
- Enable nullable reference types
- Use primary constructors where appropriate