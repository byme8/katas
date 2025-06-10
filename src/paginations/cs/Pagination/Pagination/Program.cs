using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Driver;
using Pagination.Data;
using Pagination.Json;
using Pagination.Services;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

builder.AddServiceDefaults();
builder.AddMongoDBClient("paginationdb");

services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverterProvider());
});

services.AddScoped<PaginationMongoContext>();
services.AddScoped<CommentsService>();
services.AddHostedService<DatabaseSeedingService>();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGet("/users", async (PaginationMongoContext context) =>
{
    var users = await context.Users
        .Find(u => !u.IsDeleted)
        .ToListAsync();
    return users;
});

app.MapGet("/users/{userId}/comments", async (string userId, PaginationMongoContext context) =>
{
    var comments = await context.Comments
        .Find(c => c.UserId == ObjectId.Parse(userId) && !c.IsDeleted)
        .ToListAsync();
    return comments;
});

app.MapComments();

app.Run();