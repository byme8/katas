using Microsoft.EntityFrameworkCore;
using Pagination.Data;
using Pagination.Data.Entities;

namespace Pagination.Services;

public class UsersService(PaginationDbContext context)
{
    public async Task<(List<User> users, int count)> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await context.Users
            .Where(u => !u.IsDeleted)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync(cancellationToken);
        
        return (users, users.Count);
    }
    
    public async Task<(List<User> users, int count)> GetAllCompanyUsersAsync(long companyId, CancellationToken cancellationToken = default)
    {
        var users = await context.Users
            .Where(u => u.CompanyId == companyId && !u.IsDeleted)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync(cancellationToken);
        
        return (users, users.Count);
    }
}