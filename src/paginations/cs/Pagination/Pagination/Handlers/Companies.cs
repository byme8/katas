using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pagination.Data;
using Pagination.Services;

public static class CompaniesHandler
{
    public static void MapCompanies(this IEndpointRouteBuilder app)
    {
        app.MapGet("/companies", async (PaginationDbContext context, CancellationToken cancellationToken) =>
        {
            var companies = await context.Companies
                .ToListAsync(cancellationToken);
            return companies;
        });

        app.MapPost("/companies/offset", async ([FromBody]OffsetPaginationRequest<CompanySortField> request, CompaniesOffsetService service, CancellationToken cancellationToken) =>
        {
            var validationErrors = request.Validate();
            if (validationErrors.Count > 0)
            {
                return Results.BadRequest(new { Errors = validationErrors });
            }
            
            var companies = await service.GetCompaniesAsync(request, cancellationToken);
            
            return Results.Ok(companies);
        });
        
        app.MapGet("/companies/{companyId}/users", async (long companyId, PaginationDbContext context, CancellationToken cancellationToken) =>
        {
            var users = await context.Users
                .Where(u => u.CompanyId == companyId)
                .ToListAsync(cancellationToken);
            return users;
        });
    }
}