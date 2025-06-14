using System.ComponentModel;
using System.Data;
using Apparatus.AOT.Reflection;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Pagination.Data;
using Pagination.Data.Entities;

namespace Pagination.Services;

[AOTReflection]
public enum CommentSortField
{
    [Description("NONE")] None,
    [Description("USER_ID")] UserId,
    [Description("CREATED_AT")] CreatedAt,
    [Description("UPDATED_AT")] UpdatedAt,
}

public class CommentsOffsetService(PaginationDbContext context)
{
    public async Task<OffsetPaginationResponse<Comment>> GetCommentsAsync(Guid? userId, OffsetPaginationRequest<CommentSortField> offset)
    {
        var page = offset.Page;
        var size = offset.Size;
        var orderBy = offset.OrderBy;
        var direction = offset.Direction;

        var orderByClause = CreateOrdeByClause(orderBy, direction);
        var countQuery = CreateCountOverClause(offset);
        var userClause = CreateUserClause(userId);

        var offsetValue = (page - 1) * size;
        var sql = $"""
                   SELECT c."Id", c."UserId", c."Message", c."CreatedAt", c."UpdatedAt", c."DeletedAt", c."IsDeleted"
                          {countQuery}
                   FROM "Comments" c
                   WHERE c."IsDeleted" = false
                   {userClause}
                   {orderByClause}
                   LIMIT @Size OFFSET @Offset
                   """;

        var parameters = new DynamicParameters();
        if (userId.HasValue)
        {
            parameters.Add("UserId", userId.Value);
        }

        parameters.Add("Size", size);
        parameters.Add("Offset", offsetValue);

        var connection = context.Database.GetDbConnection();

        var results = await connection.QueryAsync<CommentWithCount>(sql, parameters);
        var resultsList = results.ToList();

        var totalCount = !offset.SkipCount ? resultsList.FirstOrDefault()?.TotalCount ?? 0 : (long?)null;
        var comments = resultsList
            .Select(r => new Comment
            {
                Id = r.Id,
                UserId = r.UserId,
                Message = r.Message,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                IsDeleted = r.IsDeleted,
                DeletedAt = r.DeletedAt
            })
            .ToArray();

        return new OffsetPaginationResponse<Comment>
        {
            Data = comments,
            Metadata = new PaginationMetadata
            {
                CurrentPage = page,
                PageSize = size,
                TotalCount = totalCount,
                TotalPages = totalCount.HasValue ? (int)Math.Ceiling(totalCount.Value / (double)size) : null,
            }
        };
    }

    private string CreateUserClause(Guid? userId)
        => userId.HasValue ? @"AND c.""UserId"" = @UserId" : string.Empty;

    private static string CreateCountOverClause(OffsetPaginationRequest<CommentSortField> offset)
        => !offset.SkipCount ? ", COUNT(*) OVER() as TotalCount" : string.Empty;

    private string CreateOrdeByClause(CommentSortField orderBy, SortDirection direction)
    {
        var column = orderBy switch
        {
            CommentSortField.UserId => "c.\"UserId\"",
            CommentSortField.CreatedAt => "c.\"CreatedAt\"",
            CommentSortField.UpdatedAt => "COALESCE(c.\"UpdatedAt\", c.\"CreatedAt\")",
            _ => "c.\"CreatedAt\"" // Default fallback
        };

        var sortDirection = direction switch
        {
            SortDirection.Ascending => "ASC",
            SortDirection.Descending => "DESC",
            _ => "DESC" // Default fallback
        };

        return $"ORDER BY {column} {sortDirection}, c.\"Id\" ASC";
    }
}

internal class CommentWithCount
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted { get; set; }
    public long TotalCount { get; set; }
}