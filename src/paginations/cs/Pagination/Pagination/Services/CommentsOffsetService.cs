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

        var orderByClause = BuildOrderByClause(orderBy, direction);
        var offsetValue = (page - 1) * size;

        var sql = $"""
                   SELECT c."Id", c."UserId", c."Message", c."CreatedAt", c."UpdatedAt", c."DeletedAt", c."IsDeleted",
                          COUNT(*) OVER() as TotalCount
                   FROM "Comments" c
                   WHERE c."IsDeleted" = false
                   {(userId.HasValue ? @"AND c.""UserId"" = @UserId" : string.Empty)}
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

        var totalCount = resultsList.FirstOrDefault()?.TotalCount ?? 0;
        var comments = resultsList.Select(r => new Comment
        {
            Id = r.Id,
            UserId = r.UserId,
            Message = r.Message,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt,
            IsDeleted = r.IsDeleted,
            DeletedAt = r.DeletedAt
        }).ToList();

        return new OffsetPaginationResponse<Comment>
        {
            Data = comments,
            Page = page,
            Size = size,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)size)
        };
    }

    private string BuildOrderByClause(CommentSortField orderBy, SortDirection direction)
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

        // Always include Id for stable sorting
        return $"ORDER BY {column} {sortDirection}, c.\"Id\" ASC";
    }
}

// DTO for raw SQL query results
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