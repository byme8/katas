using Bogus;
using Microsoft.EntityFrameworkCore;
using Pagination;
using Pagination.Data;
using Pagination.Data.Entities;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddDbContext<PaginationDbContext>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PaginationDbContext>();
    context.Database.EnsureCreated();
}

app.MapGet("/users", async (PaginationDbContext context) => await context.Agents.ToArrayAsync());
app.MapGet("/users/{userId}/comments", async (int userId, PaginationDbContext context) => await context
    .Customers
    .Where(o => o.UserId == userId)
    .ToArrayAsync());

app.MapGet("/users/{userId}/comments/offset", async (int userId, int page, int size, PaginationDbContext context) => await context
    .Customers
    .Where(o => o.UserId == userId)
    .Skip((page - 1) * size)
    .Take(size)
    .ToArrayAsync());


app.Run();


namespace Pagination
{
    record User(int Id, string FirstName, string LastName, string Email);
}