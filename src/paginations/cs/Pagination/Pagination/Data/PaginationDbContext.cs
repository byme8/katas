using Microsoft.EntityFrameworkCore;
using Pagination.Data.Entities;

namespace Pagination.Data;

public class PaginationDbContext : DbContext
{
    public PaginationDbContext(DbContextOptions<PaginationDbContext> options) : base(options)
    {
    }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Company entity configuration
        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => new { e.IsDeleted, e.DeletedAt });
            
            // Soft delete query filter
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // User entity configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            entity.Property(e => e.TwitterHandle).HasMaxLength(100);
            entity.Property(e => e.FacebookProfile).HasMaxLength(200);
            entity.Property(e => e.WhatsAppNumber).HasMaxLength(50);
            entity.Property(e => e.InstagramHandle).HasMaxLength(100);
            entity.Property(e => e.BlueskyHandle).HasMaxLength(100);
            
            entity.HasIndex(e => e.CompanyId);
            entity.HasIndex(e => e.Email);
            
            // Social media field indexes for search/lookup
            entity.HasIndex(e => e.PhoneNumber);
            entity.HasIndex(e => e.TwitterHandle);
            entity.HasIndex(e => e.InstagramHandle);
            entity.HasIndex(e => e.BlueskyHandle);
            
            // Optimized indexes for single-column sorting with soft delete
            entity.HasIndex(e => new { e.IsDeleted, e.CreatedAt, e.Id })
                .HasDatabaseName("IX_Users_IsDeleted_CreatedAt_Id");
            entity.HasIndex(e => new { e.IsDeleted, e.UpdatedAt, e.Id })
                .HasDatabaseName("IX_Users_IsDeleted_UpdatedAt_Id");
            entity.HasIndex(e => new { e.IsDeleted, e.CompanyId, e.Id })
                .HasDatabaseName("IX_Users_IsDeleted_CompanyId_Id");
            
            // For soft delete cleanup queries
            entity.HasIndex(e => new { e.IsDeleted, e.DeletedAt });
            
            // Relationship
            entity.HasOne(e => e.Company)
                .WithMany(c => c.Users)
                .HasForeignKey(e => e.CompanyId)
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