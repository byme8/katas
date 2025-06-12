using Microsoft.EntityFrameworkCore;
using Pagination.Data.Entities;

namespace Pagination.Data;

public class PaginationDbContext : DbContext
{
    public PaginationDbContext(DbContextOptions<PaginationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User entity configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => new { e.IsDeleted, e.DeletedAt });
            
            // Soft delete query filter
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Comment entity configuration
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Message).IsRequired();
            entity.HasIndex(e => e.UserId);
            
            // Optimized indexes for single-column sorting with soft delete
            entity.HasIndex(e => new { e.IsDeleted, e.CreatedAt, e.Id })
                .HasDatabaseName("IX_Comments_IsDeleted_CreatedAt_Id");
            entity.HasIndex(e => new { e.IsDeleted, e.UpdatedAt, e.Id })
                .HasDatabaseName("IX_Comments_IsDeleted_UpdatedAt_Id");
            entity.HasIndex(e => new { e.IsDeleted, e.UserId, e.Id })
                .HasDatabaseName("IX_Comments_IsDeleted_UserId_Id");
            
            // For soft delete cleanup queries
            entity.HasIndex(e => new { e.IsDeleted, e.DeletedAt });
            
            // Relationship
            entity.HasOne(e => e.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Soft delete query filter
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<Entity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}