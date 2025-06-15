using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Pagination.Services;

public static class UsersHandler
{
    public static void MapUsers(this IEndpointRouteBuilder app)
    {
        app.MapPost("/companies/{companyId}/users/offset", async (long companyId, [FromBody]OffsetPaginationRequest<UserSortField> request, UsersOffsetService context, CancellationToken cancellationToken) =>
        {
            var validationErrors = request.Validate();
            if (validationErrors.Count > 0)
            {
                return Results.BadRequest(new { Errors = validationErrors });
            }
            
            var users = await context.GetUsersAsync(companyId, request, cancellationToken);
            
            return Results.Ok(users);
        });
        
        app.MapPost("/users/offset", async ([FromBody]OffsetPaginationRequest<UserSortField> request, UsersOffsetService context, CancellationToken cancellationToken) =>
        {
            var validationErrors = request.Validate();
            if (validationErrors.Count > 0)
            {
                return Results.BadRequest(new { Errors = validationErrors });
            }
            
            var users = await context.GetUsersAsync(null, request, cancellationToken);
            
            return Results.Ok(users);
        });

        app.MapPost("/users/cursor", async ([FromBody]CursorPaginationRequest<UserSortField> request, UsersCursorService cursorService, CancellationToken cancellationToken) =>
        {
            var validationErrors = request.Validate();
            if (validationErrors.Count > 0)
            {
                return Results.BadRequest(new { Errors = validationErrors });
            }
            
            var users = await cursorService.GetUsersAsync(null, request, cancellationToken);
            
            return Results.Ok(users);
        });
        
        app.MapGet("/users/all", async (UsersService usersService, CancellationToken cancellationToken) =>
        {
            var (users, count) = await usersService.GetAllUsersAsync(cancellationToken);
            
            return Results.Ok(new { data = users, count });
        });
    }
}