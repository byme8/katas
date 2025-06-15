using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Pagination.Data;
using Pagination.Json;
using Pagination.Services;
using System.Data.Common;
using Dapper;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<PaginationDbContext>("paginationdb");

services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverterProvider());
});

services.AddScoped<UsersOffsetService>();
services.AddScoped<UsersCursorService>();
services.AddScoped<CursorService>();
services.AddScoped<UsersService>();
services.AddScoped<CompaniesOffsetService>();
services.AddHostedService<DatabaseSeedingService>();

// Configure Dapper SQL logging
SqlMapper.Settings.CommandTimeout = 30;

var app = builder.Build();

app.MapDefaultEndpoints();

var api = app.MapGroup("/api");
api.MapCompanies();
api.MapUsers();

app.Run();