using System.ComponentModel;
using System.Data;
using Apparatus.AOT.Reflection;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Pagination.Data;
using Pagination.Data.Entities;

namespace Pagination.Services;

[AOTReflection]
public enum CompanySortField
{
    [Description("NONE")] None,
    [Description("ID")] Id,
    [Description("NAME")] Name,
    [Description("EMAIL")] Email,
    [Description("CREATED_AT")] CreatedAt,
    [Description("UPDATED_AT")] UpdatedAt
}

public class CompaniesOffsetService(PaginationDbContext context, ILogger<CompaniesOffsetService> logger)
{
    public async Task<OffsetPaginationResponse<Company>> GetCompaniesAsync(OffsetPaginationRequest<CompanySortField> offset, CancellationToken cancellationToken = default)
    {
        var page = offset.Page;
        var size = offset.Size;
        var orderBy = offset.OrderBy;
        var direction = offset.Direction;

        var orderByClause = CreateOrderByClause(orderBy, direction);
        var countQuery = CreateCountOverClause(offset);

        var offsetValue = (page - 1) * size;
        var sql = $"""
                   SELECT c."Id", c."Name", c."Email", c."CreatedAt", c."UpdatedAt", c."DeletedAt", c."IsDeleted"
                          {countQuery}
                   FROM "Companies" c
                   WHERE c."IsDeleted" = false
                   {orderByClause}
                   LIMIT @Size OFFSET @Offset
                   """;

        var parameters = new DynamicParameters();
        parameters.Add("Size", size);
        parameters.Add("Offset", offsetValue);

        var connection = context.Database.GetDbConnection();


        var command = new CommandDefinition(sql, parameters, commandTimeout: 30, cancellationToken: cancellationToken);
        var results = await connection.QueryAsync<CompanyWithCount>(command);
        var resultsList = results.ToList();

        var totalCount = !offset.SkipCount ? resultsList.FirstOrDefault()?.TotalCount ?? 0 : (long?)null;
        var companies = resultsList
            .Select(r => new Company
            {
                Id = r.Id,
                Name = r.Name,
                Email = r.Email,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                IsDeleted = r.IsDeleted,
                DeletedAt = r.DeletedAt
            })
            .ToArray();

        return new OffsetPaginationResponse<Company>
        {
            Data = companies,
            Metadata = new PaginationMetadata
            {
                CurrentPage = page,
                PageSize = size,
                TotalCount = totalCount,
                TotalPages = totalCount.HasValue ? (int)Math.Ceiling(totalCount.Value / (double)size) : null,
            }
        };
    }

    private static string CreateCountOverClause(OffsetPaginationRequest<CompanySortField> offset)
        => !offset.SkipCount ? ", COUNT(*) OVER() as TotalCount" : string.Empty;

    private string CreateOrderByClause(CompanySortField orderBy, SortDirection direction)
    {
        var column = orderBy switch
        {
            CompanySortField.Id => "c.\"Id\"",
            CompanySortField.Name => "c.\"Name\"",
            CompanySortField.Email => "c.\"Email\"",
            CompanySortField.CreatedAt => "c.\"CreatedAt\"",
            CompanySortField.UpdatedAt => "COALESCE(c.\"UpdatedAt\", c.\"CreatedAt\")",
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

internal class CompanyWithCount
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted { get; set; }
    public long TotalCount { get; set; }
}