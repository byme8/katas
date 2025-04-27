# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Test Commands
- Build: `dotnet build src/tinyurl/cs/TinyUrl/TinyUrl.sln`
- Run app: `dotnet run --project src/tinyurl/cs/TinyUrl/TinyUrl/TinyUrl.csproj`
- Run tests: `dotnet test src/tinyurl/cs/TinyUrl/TinyUrl.Tests/TinyUrl.Tests.csproj`
- Run single test: `dotnet test --filter "FullyQualifiedName=TinyUrl.Tests.TinyUrlTests.AddUrl"`

## Code Style Guidelines
- PascalCase for classes, methods, properties
- Use primary constructor pattern when appropriate
- Implement extension methods for registering routes
- Use static handler classes with extension methods
- Early returns for validation/guard clauses
- Enable nullable reference types
- Use asynchronous DB operations (FindAsync, SaveChangesAsync)
- Return appropriate HTTP status codes for errors
- Utilize caching for performance optimization
- Follow domain-driven naming (Entity vs Model)
- Use integration tests with WebApplicationFactory
- Implement Verify pattern for snapshot testing
- Use in-memory SQLite database for tests