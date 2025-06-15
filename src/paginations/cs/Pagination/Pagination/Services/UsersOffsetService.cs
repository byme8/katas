using System.ComponentModel;
using System.Data;
using Apparatus.AOT.Reflection;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Pagination.Data;
using Pagination.Data.Entities;

namespace Pagination.Services;

[AOTReflection]
public enum UserSortField
{
    [Description("NONE")] None,
    [Description("COMPANY_ID")] CompanyId,
    [Description("NAME")] Name,
    [Description("EMAIL")] Email,
    [Description("PHONE_NUMBER")] PhoneNumber,
    [Description("TWITTER_HANDLE")] TwitterHandle,
    [Description("FACEBOOK_PROFILE")] FacebookProfile,
    [Description("WHATSAPP_NUMBER")] WhatsAppNumber,
    [Description("INSTAGRAM_HANDLE")] InstagramHandle,
    [Description("BLUESKY_HANDLE")] BlueskyHandle,
    [Description("CREATED_AT")] CreatedAt,
    [Description("UPDATED_AT")] UpdatedAt,
}

public class UsersOffsetService(PaginationDbContext context, ILogger<UsersOffsetService> logger)
{
    public async Task<OffsetPaginationResponse<User>> GetUsersAsync(long? companyId, OffsetPaginationRequest<UserSortField> offset, CancellationToken cancellationToken = default)
    {
        var page = offset.Page;
        var size = offset.Size;
        var orderBy = offset.OrderBy;
        var direction = offset.Direction;

        var orderByClause = CreateOrdeByClause(orderBy, direction);
        var countQuery = CreateCountOverClause(offset);
        var companyClause = CreateCompanyClause(companyId);

        var offsetValue = (page - 1) * size;
        var sql = $"""
                   SELECT u."Id", u."CompanyId", u."Name", u."Email", u."PhoneNumber", u."TwitterHandle", 
                          u."FacebookProfile", u."WhatsAppNumber", u."InstagramHandle", u."BlueskyHandle",
                          u."CreatedAt", u."UpdatedAt", u."DeletedAt", u."IsDeleted"
                          {countQuery}
                   FROM "Users" u
                   WHERE u."IsDeleted" = false
                   {companyClause}
                   {orderByClause}
                   LIMIT @Size OFFSET @Offset
                   """;

        var parameters = new DynamicParameters();
        if (companyId.HasValue)
        {
            parameters.Add("CompanyId", companyId.Value);
        }

        parameters.Add("Size", size);
        parameters.Add("Offset", offsetValue);

        var connection = context.Database.GetDbConnection();


        var command = new CommandDefinition(sql, parameters, commandTimeout: 30, cancellationToken: cancellationToken);
        var results = await connection.QueryAsync<UserWithCount>(command);
        var resultsList = results.ToList();

        var totalCount = !offset.SkipCount ? resultsList.FirstOrDefault()?.TotalCount ?? 0 : (long?)null;
        var users = resultsList
            .Select(r => new User
            {
                Id = r.Id,
                CompanyId = r.CompanyId,
                Name = r.Name,
                Email = r.Email,
                PhoneNumber = r.PhoneNumber,
                TwitterHandle = r.TwitterHandle,
                FacebookProfile = r.FacebookProfile,
                WhatsAppNumber = r.WhatsAppNumber,
                InstagramHandle = r.InstagramHandle,
                BlueskyHandle = r.BlueskyHandle,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                IsDeleted = r.IsDeleted,
                DeletedAt = r.DeletedAt
            })
            .ToArray();

        return new OffsetPaginationResponse<User>
        {
            Data = users,
            Metadata = new PaginationMetadata
            {
                CurrentPage = page,
                PageSize = size,
                TotalCount = totalCount,
                TotalPages = totalCount.HasValue ? (int)Math.Ceiling(totalCount.Value / (double)size) : null,
            }
        };
    }

    private string CreateCompanyClause(long? companyId)
        => companyId.HasValue ? """ AND u."CompanyId" = @CompanyId """ : string.Empty;

    private static string CreateCountOverClause(OffsetPaginationRequest<UserSortField> offset)
        => !offset.SkipCount ? ", COUNT(*) OVER() as TotalCount" : string.Empty;

    private string CreateOrdeByClause(UserSortField orderBy, SortDirection direction)
    {
        var column = orderBy switch
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

        var sortDirection = direction switch
        {
            SortDirection.Ascending => "ASC",
            SortDirection.Descending => "DESC",
            _ => "DESC" // Default fallback
        };

        return $"""ORDER BY {column} {sortDirection}, u."Id" ASC""";
    }
}

internal class UserWithCount
{
    public long Id { get; set; }
    public long CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? TwitterHandle { get; set; }
    public string? FacebookProfile { get; set; }
    public string? WhatsAppNumber { get; set; }
    public string? InstagramHandle { get; set; }
    public string? BlueskyHandle { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted { get; set; }
    public long TotalCount { get; set; }
}