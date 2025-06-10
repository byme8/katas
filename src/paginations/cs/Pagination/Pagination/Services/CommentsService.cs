using System.ComponentModel;
using Apparatus.AOT.Reflection;
using MongoDB.Bson;
using MongoDB.Driver;
using Pagination.Data;
using Pagination.Data.MongoEntities;

namespace Pagination.Services;

[AOTReflection]
public enum CommentSortField
{
    [Description("NONE")] None,
    [Description("USER_ID")] UserId,
    [Description("CREATED_AT")] CreatedAt,
    [Description("UPDATED_AT")] UpdatedAt,
}

public class CommentsService(PaginationMongoContext context)
{
    public async Task<OffsetPaginationResponse<Comment>> GetCommentsAsync(string? userId, OffsetPaginationRequest<CommentSortField, SortDirection> offset)
    {
        var page = offset.Page;
        var size = offset.Size;
        var ordering = offset.Ordering;

        var baseFilter = userId is null
            ? Builders<Comment>.Filter.Empty
            : Builders<Comment>.Filter.Eq("userId", ObjectId.Parse(userId));

        var isDeletedFilter = Builders<Comment>.Filter.Eq("isDeleted", false);

        var filter = Builders<Comment>.Filter.And(baseFilter, isDeletedFilter);
        var sortDefinitions = ordering.Select(o =>
        {
            var orderDirection = o.Direction;
            var orderBy = o.OrderBy;

            var sortField = orderBy switch
            {
                CommentSortField.UserId => "userId",
                CommentSortField.CreatedAt => "createdAt",
                CommentSortField.UpdatedAt => "updatedAt",
                _ => throw new ArgumentOutOfRangeException(nameof(orderDirection), orderDirection, null)
            };

            var sortDefinition = orderDirection switch
            {
                SortDirection.Ascending => Builders<Comment>.Sort.Ascending(sortField),
                SortDirection.Descending => Builders<Comment>.Sort.Descending(sortField),
                _ => throw new ArgumentOutOfRangeException(nameof(orderBy), orderBy, null)
            };

            return sortDefinition;
        });

        var query = context.Comments
            .Find(filter)
            .Sort(Builders<Comment>.Sort.Combine(sortDefinitions))
            .Skip((page - 1) * size)
            .Limit(size);

        var comments = await query.ToListAsync();

        // var totalCount = await context.Comments.CountDocumentsAsync(filter);

        return new OffsetPaginationResponse<Comment>
        {
            Data = comments,
            Page = page,
            Size = size,
            // TotalCount = totalCount,
            // TotalPages = (int)Math.Ceiling(totalCount / (double)size)
        };
    }
}