# Code Katas Repository

This repository contains implementations of various coding katas in different programming languages.

## Available Katas

### TinyURL

A URL shortener service implementation similar to TinyURL or Bit.ly. The implementation allows:
- Creating short URLs from long URLs
- Redirecting from short URLs to their original long URLs
- Efficient storage using a database
- Performance optimization with caching

#### C# Implementation

The C# implementation features:
- ASP.NET Core minimal API
- Entity Framework Core with SQLite database
- In-memory caching for faster redirects
- Base62 ID encoding for shorter URLs
- Integration tests using WebApplicationFactory
- Snapshot testing with Verify

#### Build & Run

```bash
# Build
dotnet build src/tinyurl/cs/TinyUrl/TinyUrl.sln

# Run application
dotnet run --project src/tinyurl/cs/TinyUrl/TinyUrl/TinyUrl.csproj

# Run tests
dotnet test src/tinyurl/cs/TinyUrl/TinyUrl.Tests/TinyUrl.Tests.csproj
```