# MongoDB Pagination with .NET Aspire

This project demonstrates pagination techniques using MongoDB and .NET Aspire.

## Prerequisites

- .NET 9.0 SDK
- Docker Desktop (for MongoDB)
- .NET Aspire workload

## Installation

1. Install .NET Aspire workload:
```bash
dotnet workload install aspire
```

2. Restore dependencies:
```bash
dotnet restore
```

## Running the Application

### With Aspire (Recommended)

Run the Aspire host which will start MongoDB in a container and the API:

```bash
dotnet run --project Pagination.AppHost/Pagination.AppHost.csproj
```

This will:
- Start MongoDB in a Docker container
- Launch the Pagination API
- Open the Aspire dashboard in your browser
- Automatically seed the database with 10,000 users and 1,000,000 comments

### Direct Run (Requires MongoDB)

If you have MongoDB running locally:

```bash
dotnet run --project Pagination/Pagination.csproj
```

## API Endpoints

### Get All Users
```
GET /users
```

### Get Comments for a User
```
GET /users/{userId}/comments
```

### Offset-based Pagination
```
GET /users/{userId}/comments/offset?page=1&size=10
```
Returns:
- `data`: Array of comments
- `page`: Current page number
- `size`: Page size
- `totalCount`: Total number of comments
- `totalPages`: Total number of pages

### Cursor-based Pagination (Recommended for large datasets)
```
GET /users/{userId}/comments/cursor?size=10
GET /users/{userId}/comments/cursor?cursor={previousCursor}&size=10
```
Returns:
- `data`: Array of comments
- `nextCursor`: Cursor for the next page
- `hasMore`: Boolean indicating if more data exists

## Architecture

- **MongoDB**: Document database for storing users and comments
- **.NET Aspire**: Orchestrates the application and MongoDB container
- **Seeding**: Automatically seeds 10,000 users with 100 comments each (1M total comments)
- **Indexes**: Created on userId, email, and soft delete fields for performance

## Testing

Use the `Pagination.http` file with REST Client extension in VS Code or any HTTP client to test the endpoints.

Note: Replace `{userId}` in the examples with an actual MongoDB ObjectId from the `/users` endpoint response.