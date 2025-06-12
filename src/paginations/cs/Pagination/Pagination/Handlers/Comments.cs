using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Pagination.Services;

public static class CommentsHandler
{
    public static void MapComments(this WebApplication app)
    {
        app.MapPost("/users/{userId}/comments/offset", async (Guid userId, [FromBody]OffsetPaginationRequest<CommentSortField> request, CommentsOffsetService context) =>
        {
            var validationErrors = request.Validate();
            if (validationErrors.Count > 0)
            {
                return Results.BadRequest(new { Errors = validationErrors });
            }
            
            var comments = await context.GetCommentsAsync(userId, request);
            
            return Results.Ok(comments);
        });
        
        app.MapPost("/comments/offset", async ([FromBody]OffsetPaginationRequest<CommentSortField> request, CommentsOffsetService context) =>
        {
            var validationErrors = request.Validate();
            if (validationErrors.Count > 0)
            {
                return Results.BadRequest(new { Errors = validationErrors });
            }
            
            var comments = await context.GetCommentsAsync(null, request);
            
            return Results.Ok(comments);
        });
    }
}