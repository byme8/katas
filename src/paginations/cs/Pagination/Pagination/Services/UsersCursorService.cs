using System.ComponentModel;
using System.Data;
using Apparatus.AOT.Reflection;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Pagination.Data;
using Pagination.Data.Entities;

namespace Pagination.Services;

public class UsersCursorService(PaginationDbContext context, CursorService cursorService, ILogger<UsersCursorService> logger)
{
    public async Task<CursorPaginationResponse<User>> GetUsersAsync(long? companyId, CursorPaginationRequest<UserSortField> cursor, CancellationToken cancellationToken = default)
    {
        var size = cursor.Size;
        var orderBy = cursor.OrderBy;
        var direction = cursor.Direction;
        var cursorValue = cursor.Cursor;

        // Determine backward flag from cursor content if available
        var isBackward = cursor.Backward;
        if (!string.IsNullOrEmpty(cursorValue))
        {
            var decodedCursor = cursorService.DecodeCursor(cursorValue);
            if (decodedCursor != null)
            {
                isBackward = decodedCursor.IsBackward;
            }
        }
        
        var (whereClause, orderByClause) = CreateCursorClauses(orderBy, direction, cursorValue, isBackward);
        var companyClause = CreateCompanyClause(companyId);

        var sql = $"""
                   SELECT u."Id", u."CompanyId", u."Name", u."Email", u."PhoneNumber", u."TwitterHandle", 
                          u."FacebookProfile", u."WhatsAppNumber", u."InstagramHandle", u."BlueskyHandle",
                          u."CreatedAt", u."UpdatedAt", u."DeletedAt", u."IsDeleted"
                   FROM "Users" u
                   WHERE u."IsDeleted" = false
                   {companyClause}
                   {whereClause}
                   {orderByClause}
                   LIMIT @size
                   """;

        var parameters = new DynamicParameters();
        if (companyId.HasValue)
        {
            parameters.Add("companyid", companyId.Value);
        }

        parameters.Add("size", size + 1); // Fetch one extra

        if (!string.IsNullOrEmpty(cursorValue))
        {
            var decodedCursor = cursorService.DecodeCursor(cursorValue);
            if (decodedCursor != null)
            {
                parameters.Add("cursorvalue", decodedCursor.Value);
                parameters.Add("cursorid", decodedCursor.Id);
            }
        }

        var connection = context.Database.GetDbConnection();


        var command = new CommandDefinition(sql, parameters, commandTimeout: 30, cancellationToken: cancellationToken);
        var results = await connection.QueryAsync<User>(command);
        var resultsList = results.ToList();

        // Determine if there are more pages
        var hasMorePages = resultsList.Count > size;
        if (hasMorePages)
        {
            resultsList.RemoveAt(resultsList.Count - 1); // Remove the extra record
        }

        // For backward pagination, reverse the results to maintain correct order
        if (isBackward)
        {
            resultsList.Reverse();
        }

        // Create cursors based on navigation direction
        var nextCursor = GetNextCursor(resultsList, hasMorePages, orderBy, isBackward);
        var previousCursor = GetPreviousCursor(resultsList, cursorValue, orderBy, isBackward);
        var firstPageCursor = cursorService.EncodeFirstPageCursor(orderBy, direction);
        var lastPageCursor = cursorService.EncodeLastPageCursor(orderBy, direction);

        return new CursorPaginationResponse<User>
        {
            Data = resultsList,
            Metadata = new CursorMetadata
            {
                PageSize = size,
                NextCursor = nextCursor,
                PreviousCursor = previousCursor,
                FirstPageCursor = firstPageCursor,
                LastPageCursor = lastPageCursor
            }
        };
    }

    private string CreateCompanyClause(long? companyId)
        => companyId.HasValue ? """ AND u."CompanyId" = @companyid """ : string.Empty;

    private (string whereClause, string orderByClause) CreateCursorClauses(UserSortField orderBy, SortDirection direction, string? cursorValue, bool backward)
    {
        var column = GetColumnName(orderBy);
        var sortDirection = direction == SortDirection.Ascending ? "ASC" : "DESC";
        var comparisonOperator = direction == SortDirection.Ascending ? ">" : "<";
        var idComparisonOperator = ">";

        // For backward pagination, reverse the comparison logic
        if (backward)
        {
            comparisonOperator = direction == SortDirection.Ascending ? "<" : ">";
            idComparisonOperator = "<";
            sortDirection = direction == SortDirection.Ascending ? "DESC" : "ASC";
        }

        var whereClause = string.Empty;
        if (!string.IsNullOrEmpty(cursorValue))
        {
            // Handle type-specific comparisons for cursor pagination
            var cursorColumnCast = GetCursorColumnCast(orderBy);
            whereClause = $"""AND (({column} {comparisonOperator} {cursorColumnCast}) OR ({column} = {cursorColumnCast} AND u."Id" {idComparisonOperator} @cursorid))""";
        }

        var orderByClause = $"""ORDER BY {column} {sortDirection}, u."Id" {(backward ? "DESC" : "ASC")}""";

        return (whereClause, orderByClause);
    }

    private string GetCursorColumnCast(UserSortField orderBy)
    {
        return orderBy switch
        {
            UserSortField.CreatedAt => "to_timestamp(@cursorvalue / 1000.0)",
            UserSortField.UpdatedAt => "to_timestamp(@cursorvalue / 1000.0)", 
            UserSortField.CompanyId => "@cursorvalue::bigint",
            _ => "@cursorvalue" // For string fields, no cast needed
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

    private string? GetNextCursor(List<User> results, bool hasMorePages, UserSortField orderBy, bool isBackward)
    {
        if (!hasMorePages || results.Count == 0) return null;

        // For backward navigation, "next" means continuing backward
        return isBackward
            ? cursorService.EncodeUserCursor(results.First(), orderBy, true)
            : cursorService.EncodeUserCursor(results.Last(), orderBy, false);
    }

    private string? GetPreviousCursor(List<User> results, string? currentCursor, UserSortField orderBy, bool isBackward)
    {
        if (string.IsNullOrEmpty(currentCursor) || results.Count == 0) return null;

        // For backward navigation, "previous" means continuing forward
        return isBackward
            ? cursorService.EncodeUserCursor(results.Last(), orderBy, false)
            : cursorService.EncodeUserCursor(results.First(), orderBy, true);
    }
}