using Bogus;
using Microsoft.EntityFrameworkCore;
using Pagination.Data.Entities;

namespace Pagination.Data;

public class PaginationDbContext: DbContext
{
    public DbSet<UserEntity> Agents { get; set; }
    
    public DbSet<CommentEntity> Customers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        
        optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
        optionsBuilder.UseSqlite("Data Source=pagination.db");
        optionsBuilder.UseSeeding((o, oo) =>
        {
            var pagination = o as PaginationDbContext;
            if (pagination == null)
            {
                return;
            }
            
            var agentsCount = pagination.Agents.Count();
            if (agentsCount > 0)
            {
                return;
            }
            
            var names = new Bogus.DataSets.Name();
            var messages = new Bogus.DataSets.Lorem();
            Randomizer.Seed = new Random(42);

            var smallAgentName = names.FullName();
            var smallAgent = new UserEntity
            {
                Name = smallAgentName,
                Email = smallAgentName.ToLowerInvariant() + "@notgmail.com",
            };

            var largeAgentName = names.FullName();
            var largeAgent = new UserEntity
            {
                Name = largeAgentName,
                Email = largeAgentName.ToLowerInvariant() + "@notgmail.com",
            };

            var smallAgentEntity = o.Add(smallAgent);
            var largeAgentEntity = o.Add(largeAgent);
            o.SaveChanges();

            for (int i = 1; i < 100; i++)
            {
                var message = messages.Sentences();
                var customer = new CommentEntity()
                {
                    UserId = smallAgentEntity.Entity.Id,
                    Message = message,
                };

                o.Add(customer);
            }

            for (int i = 1; i < 100; i++)
            {
                for (int j = 1; j < 100; j++)
                {
                    var message = messages.Sentences();
                    var customer = new CommentEntity()
                    {
                        UserId = largeAgentEntity.Entity.Id,
                        Message = message,
                    };
                    o.Add(customer);
                }

                o.SaveChanges();
            }
        });
        
        base.OnConfiguring(optionsBuilder);
        
    }
    
    public override int SaveChanges()
    {
        HandleAuditFields();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        HandleAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void HandleAuditFields()
    {
        var entries = ChangeTracker.Entries<Entity>();
        var now = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                    break;
                
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    // Prevent changing CreatedAt
                    entry.Property(x => x.CreatedAt).IsModified = false;
                    break;
                
                case EntityState.Deleted:
                    // Soft delete instead of hard delete
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = now;
                    break;
            }
        }
    }
}