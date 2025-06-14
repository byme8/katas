using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Pagination.Data;
using Pagination.Json;
using Pagination.Services;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<PaginationDbContext>("paginationdb");

services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverterProvider());
});

services.AddScoped<CommentsOffsetService>();
services.AddScoped<CommentsService>();
services.AddHostedService<DatabaseSeedingService>();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGet("/users", async (PaginationDbContext context) =>
{
    var users = await context.Users
        .ToListAsync();
    return users;
});

app.MapGet("/users/{userId}/comments", async (Guid userId, PaginationDbContext context) =>
{
    var comments = await context.Comments
        .Where(c => c.UserId == userId)
        .ToListAsync();
    return comments;
});

app.MapComments();

app.Run();