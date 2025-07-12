using System.ComponentModel;
using System.Data;
using Apparatus.AOT.Reflection;
using Dapper;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Pagination.Data;
using Pagination.Data.Entities;

namespace Pagination.Services;

public class UsersCursorService(PaginationDbContext context, CursorService cursorService, ILogger<UsersCursorService> logger)
{
    public async Task<CursorPaginationResponse<User>> GetUsersAsync(long? companyId, CursorPaginationRequest<UserSortField> cursor, CancellationToken cancellationToken = default)
    {
        var size = cursor.Size;
        var order = cursor.Order;
        var cursorValue = cursor.Cursor;

        var decodedCursor = cursorService.DecodeCursor<UserSortField>(cursorValue);
        var (whereClause, orderByClause) = CreateCursorClauses(order, decodedCursor);
        var companyClause = CreateCompanyClause(companyId);
        var sizePlusOne = size + 1;

        var sql = $"""
                   SELECT u."Id", u."CompanyId", u."Name", u."Email", u."PhoneNumber", u."TwitterHandle", 
                          u."FacebookProfile", u."WhatsAppNumber", u."InstagramHandle", u."BlueskyHandle",
                          u."CreatedAt", u."UpdatedAt", u."DeletedAt", u."IsDeleted"
                   FROM "Users" u
                   WHERE u."IsDeleted" = false
                   {companyClause}
                   {whereClause}
                   {orderByClause}
                   LIMIT @SizePlusOne
                   """;

        var parameters = new DynamicParameters();
        if (companyId.HasValue)
        {
            parameters.Add("CompanyId", companyId.Value);
        }

        if (decodedCursor is not null)
        {
            parameters.Add("CursorId", decodedCursor.Id);
            parameters.Add("CursorValue", decodedCursor.Value);
        }

        parameters.Add("SizePlusOne", sizePlusOne);

        var connection = context.Database.GetDbConnection();
        var command = new CommandDefinition(sql, parameters, commandTimeout: 30, cancellationToken: cancellationToken);
        var allResults = await connection.QueryAsync<User>(command);
        var allResultsList = allResults.ToArray();

        var hasMorePages = allResultsList.Length > size;
        var results = allResultsList
            .Take(size)
            .ToArray();

        var firstPage = decodedCursor is null;
        var pages = new List<CursorPage>();
        if (!firstPage)
        {
            pages.Add(new CursorPage("<<", string.Empty));
        }

        if (!firstPage)
        {
            var previousSortValue = decodedCursor!.Value;
            var previousDirection = decodedCursor.Direction;
            var previousId = decodedCursor.Id;

            var cursorData = cursorService.EncodeCursor(decodedCursor.OrderBy, previousSortValue, previousDirection, previousId);
            pages.Add(new CursorPage("<", cursorData));
        }

        if (hasMorePages)
        {
            var lastResult = allResultsList.LastOrDefault();
            if (lastResult is not null)
            {
                var column = order?.OrderBy ?? decodedCursor?.OrderBy ?? UserSortField.CreatedAt;
                var direction = order?.Direction ?? decodedCursor?.Direction ?? OrderDirection.Descending;
                var lastSortValue = GetUserSortValue(lastResult, column);
                var cursorData = cursorService.EncodeCursor(column, lastSortValue, direction, lastResult.Id);
                pages.Add(new CursorPage(">", cursorData));
            }
        }

        return new CursorPaginationResponse<User>
        {
            Data = results,
            Metadata = new CursorMetadata
            {
                PageSize = size,
                Pages = pages
            }
        };
    }

    private object GetUserSortValue(User user, UserSortField orderBy)
    {
        return orderBy switch
        {
            UserSortField.CompanyId => user.CompanyId,
            UserSortField.Name => user.Name,
            UserSortField.Email => user.Email,
            UserSortField.PhoneNumber => user.PhoneNumber ?? string.Empty,
            UserSortField.TwitterHandle => user.TwitterHandle ?? string.Empty,
            UserSortField.FacebookProfile => user.FacebookProfile ?? string.Empty,
            UserSortField.WhatsAppNumber => user.WhatsAppNumber ?? string.Empty,
            UserSortField.InstagramHandle => user.InstagramHandle ?? string.Empty,
            UserSortField.BlueskyHandle => user.BlueskyHandle ?? string.Empty,
            UserSortField.CreatedAt => user.CreatedAt.ToUnixTimeMilliseconds(),
            UserSortField.UpdatedAt => (user.UpdatedAt ?? user.CreatedAt).ToUnixTimeMilliseconds(),
            _ => user.CreatedAt.ToUnixTimeMilliseconds()
        };
    }

    private string CreateCompanyClause(long? companyId)
        => companyId.HasValue ? """ AND u."CompanyId" = @companyid """ : string.Empty;

    private (string whereClause, string orderByClause) CreateCursorClauses(Order<UserSortField>? order, CursorData<UserSortField>? cursor)
    {
        if (cursor is not null)
        {
            var cursorColumn = GetColumnName(cursor.OrderBy);
            var cursorColumnCast = GetCursorColumnCast(cursor.OrderBy);

            var comparisonDirection = cursor.Direction == OrderDirection.Ascending ? "ASC" : "DESC";
            var comparisonOperator = cursor.Direction == OrderDirection.Ascending ? ">=" : "<=";
            var whereClause = $"""AND ({cursorColumn}, u."Id") {comparisonOperator} ({cursorColumnCast}, @CursorId)""";
            var orderByClause = $"""ORDER BY {cursorColumn} {comparisonDirection}, u."Id" {comparisonDirection}""";

            return (whereClause, orderByClause);
        }

        if (order is not null)
        {
            var orderBy = order.OrderBy;
            var direction = order.Direction;

            var column = GetColumnName(orderBy);
            var sortDirection = direction == OrderDirection.Ascending ? "ASC" : "DESC";

            var whereClause = string.Empty;
            var orderByClause = $"""ORDER BY {column} {sortDirection}, u."Id" {sortDirection}""";

            return (whereClause, orderByClause);
        }

        return (string.Empty, string.Empty);
    }

    private string GetCursorColumnCast(UserSortField orderBy)
    {
        return orderBy switch
        {
            UserSortField.CreatedAt => "to_timestamp(@CursorValue)",
            UserSortField.UpdatedAt => "to_timestamp(@CursorValue)",
            UserSortField.CompanyId => "@CursorValue::bigint",
            _ => "@CursorValue" // For string fields, no cast needed
        };
    }

    private string GetColumnName(UserSortField orderBy)
    {
        return orderBy switch
        {
            UserSortField.CompanyId => """ u."CompanyId" """,
            UserSortField.Name => """ u."Name" """,
            UserSortField.Email => """ u."Email" """,
            UserSortField.PhoneNumber => """ u."PhoneNumber" """,
            UserSortField.TwitterHandle => """ u."TwitterHandle" """,
            UserSortField.FacebookProfile => """ u."FacebookProfile" """,
            UserSortField.WhatsAppNumber => """ u."WhatsAppNumber" """,
            UserSortField.InstagramHandle => """ u."InstagramHandle" """,
            UserSortField.BlueskyHandle => """ u."BlueskyHandle" """,
            UserSortField.CreatedAt => """ u."CreatedAt" """,
            UserSortField.UpdatedAt => """ COALESCE(u."UpdatedAt", u."CreatedAt") """,
            _ => """ u."CreatedAt" """ // Default fallback
        };
    }
}