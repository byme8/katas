using Microsoft.EntityFrameworkCore;
using Pagination.Data;
using Pagination.Data.Entities;

namespace Pagination.Services;

public class CommentsService(PaginationDbContext context)
{
    public async Task<(List<Comment> comments, int count)> GetAllCommentsAsync()
    {
        var comments = await context.Comments
            .Where(c => !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
        
        return (comments, comments.Count);
    }
    
    public async Task<(List<Comment> comments, int count)> GetAllUserCommentsAsync(Guid userId)
    {
        var comments = await context.Comments
            .Where(c => c.UserId == userId && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
        
        return (comments, comments.Count);
    }
}