using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Pagination.Data;
using Pagination.Json;
using Pagination.Services;
using System.Data.Common;
using System.Data;
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

// Configure Dapper type handlers for NodaTime
SqlMapper.AddTypeHandler(new InstantTypeHandler());
SqlMapper.AddTypeHandler(new NullableInstantTypeHandler());

// Configure Dapper SQL logging
SqlMapper.Settings.CommandTimeout = 30;

var app = builder.Build();

app.MapDefaultEndpoints();

var api = app.MapGroup("/api");
api.MapCompanies();
api.MapUsers();

app.Run();

public class InstantTypeHandler : SqlMapper.TypeHandler<Instant>
{
    public override void SetValue(IDbDataParameter parameter, Instant value)
    {
        parameter.Value = value.ToDateTimeUtc();
        parameter.DbType = DbType.DateTime2;
    }

    public override Instant Parse(object value)
    {
        return value switch
        {
            DateTime dateTime => Instant.FromDateTimeUtc(dateTime),
            DateTimeOffset dateTimeOffset => Instant.FromDateTimeOffset(dateTimeOffset),
            _ => throw new InvalidCastException($"Cannot convert {value.GetType()} to Instant")
        };
    }
}

public class NullableInstantTypeHandler : SqlMapper.TypeHandler<Instant?>
{
    public override void SetValue(IDbDataParameter parameter, Instant? value)
    {
        parameter.Value = value?.ToDateTimeUtc() ?? (object)DBNull.Value;
        parameter.DbType = DbType.DateTime2;
    }

    public override Instant? Parse(object value)
    {
        if (value == null || value == DBNull.Value)
            return null;

        return value switch
        {
            DateTime dateTime => Instant.FromDateTimeUtc(dateTime),
            DateTimeOffset dateTimeOffset => Instant.FromDateTimeOffset(dateTimeOffset),
            _ => throw new InvalidCastException($"Cannot convert {value.GetType()} to Instant?")
        };
    }
}