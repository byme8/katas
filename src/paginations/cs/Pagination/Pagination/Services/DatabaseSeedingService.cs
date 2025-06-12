using Bogus;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Pagination.Data;
using Pagination.Data.Entities;
using System.Data;
using System.Diagnostics;

namespace Pagination.Services;

public class DatabaseSeedingService(IServiceProvider serviceProvider, ILogger<DatabaseSeedingService> logger)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting database seeding service");

        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PaginationDbContext>();
        
        // Apply migrations
        await context.Database.MigrateAsync(cancellationToken);
        
        await SeedDataAsync(context, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
    
    private async Task SeedDataAsync(PaginationDbContext context, CancellationToken cancellationToken)
    {
        try
        {
            var usersCount = await context.Users.CountAsync(cancellationToken);
            if (usersCount > 0)
            {
                logger.LogInformation("Database already seeded with {Count} users", usersCount);
                return;
            }

            logger.LogInformation("Starting database seeding...");
            var stopwatch = Stopwatch.StartNew();

            // Use raw SQL for bulk insert with COPY command - much faster than EF Core
            await BulkInsertWithCopyAsync(context, cancellationToken);
            
            stopwatch.Stop();
            logger.LogInformation("Database seeding completed in {Elapsed}s", stopwatch.Elapsed.TotalSeconds);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during database seeding");
            throw;
        }
    }
    
    private async Task BulkInsertWithCopyAsync(PaginationDbContext context, CancellationToken cancellationToken)
    {
        var connectionString = context.Database.GetConnectionString();
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var names = new Bogus.DataSets.Name();
        var messages = new Bogus.DataSets.Lorem();
        Randomizer.Seed = new Random(42);

        const int totalUsers = 100_000;
        const int commentsPerUser = 1000;
        const int batchSize = 10_000;
        
        var baseTime = DateTime.UtcNow;

        // Disable indexes and constraints temporarily for faster inserts
        await using (var cmd = new NpgsqlCommand(@"
            ALTER TABLE ""Comments"" DROP CONSTRAINT IF EXISTS ""FK_Comments_Users_UserId"";
            DROP INDEX IF EXISTS ""IX_Comments_UserId"";
            DROP INDEX IF EXISTS ""IX_Comments_IsDeleted_DeletedAt"";
            DROP INDEX IF EXISTS ""IX_Comments_IsDeleted_CreatedAt_Id"";
            DROP INDEX IF EXISTS ""IX_Comments_IsDeleted_UpdatedAt_Id"";
            DROP INDEX IF EXISTS ""IX_Comments_IsDeleted_UserId_Id"";
            DROP INDEX IF EXISTS ""IX_Users_Email"";
            DROP INDEX IF EXISTS ""IX_Users_IsDeleted_DeletedAt"";", connection))
        {
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        // Bulk insert users
        logger.LogInformation("Starting bulk insert of {Count} users", totalUsers);
        await using (var writer = await connection.BeginBinaryImportAsync(
            @"COPY ""Users"" (""Id"", ""Name"", ""Email"", ""CreatedAt"", ""UpdatedAt"", ""IsDeleted"", ""DeletedAt"") FROM STDIN (FORMAT BINARY)", 
            cancellationToken))
        {
            for (int i = 0; i < totalUsers; i++)
            {
                var name = names.FullName();
                var userId = Guid.NewGuid();
                
                await writer.StartRowAsync(cancellationToken);
                await writer.WriteAsync(userId, NpgsqlTypes.NpgsqlDbType.Uuid, cancellationToken);
                await writer.WriteAsync(name, NpgsqlTypes.NpgsqlDbType.Text, cancellationToken);
                await writer.WriteAsync($"{name.ToLowerInvariant().Replace(" ", ".")}@example.com", NpgsqlTypes.NpgsqlDbType.Text, cancellationToken);
                await writer.WriteAsync(baseTime, NpgsqlTypes.NpgsqlDbType.TimestampTz, cancellationToken);
                await writer.WriteAsync(baseTime, NpgsqlTypes.NpgsqlDbType.TimestampTz, cancellationToken);
                await writer.WriteAsync(false, NpgsqlTypes.NpgsqlDbType.Boolean, cancellationToken);
                await writer.WriteAsync(DBNull.Value, NpgsqlTypes.NpgsqlDbType.TimestampTz, cancellationToken);
                
                if ((i + 1) % 10000 == 0)
                {
                    logger.LogInformation("Inserted {Count} users", i + 1);
                }
            }
            
            await writer.CompleteAsync(cancellationToken);
        }

        // Get all user IDs for comments
        var userIds = new List<Guid>();
        await using (var cmd = new NpgsqlCommand(@"SELECT ""Id"" FROM ""Users""", connection))
        await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                userIds.Add(reader.GetGuid(0));
            }
        }

        // Bulk insert comments in batches
        logger.LogInformation("Starting bulk insert of {Count} comments", totalUsers * commentsPerUser);
        var commentCount = 0;
        
        for (int batch = 0; batch < totalUsers; batch += batchSize)
        {
            var userBatch = userIds.Skip(batch).Take(batchSize).ToList();
            
            await using var writer = await connection.BeginBinaryImportAsync(
                @"COPY ""Comments"" (""Id"", ""UserId"", ""Message"", ""CreatedAt"", ""UpdatedAt"", ""IsDeleted"", ""DeletedAt"") FROM STDIN (FORMAT BINARY)", 
                cancellationToken);
            
            foreach (var userId in userBatch)
            {
                for (int j = 0; j < commentsPerUser; j++)
                {
                    var message = messages.Sentences();
                    var createdAt = baseTime.AddMinutes(-Random.Shared.Next(0, 10000));
                    var updatedAt = baseTime.AddMinutes(-Random.Shared.Next(0, 10000));
                    
                    await writer.StartRowAsync(cancellationToken);
                    await writer.WriteAsync(Guid.NewGuid(), NpgsqlTypes.NpgsqlDbType.Uuid, cancellationToken);
                    await writer.WriteAsync(userId, NpgsqlTypes.NpgsqlDbType.Uuid, cancellationToken);
                    await writer.WriteAsync(message, NpgsqlTypes.NpgsqlDbType.Text, cancellationToken);
                    await writer.WriteAsync(createdAt, NpgsqlTypes.NpgsqlDbType.TimestampTz, cancellationToken);
                    await writer.WriteAsync(updatedAt, NpgsqlTypes.NpgsqlDbType.TimestampTz, cancellationToken);
                    await writer.WriteAsync(false, NpgsqlTypes.NpgsqlDbType.Boolean, cancellationToken);
                    await writer.WriteAsync(DBNull.Value, NpgsqlTypes.NpgsqlDbType.TimestampTz, cancellationToken);
                    
                    commentCount++;
                }
            }
            
            await writer.CompleteAsync(cancellationToken);
            logger.LogInformation("Inserted {Count} comments", commentCount);
        }

        // Re-create indexes and constraints
        logger.LogInformation("Recreating indexes and constraints");
        await using (var cmd = new NpgsqlCommand(@"
            CREATE INDEX ""IX_Users_Email"" ON ""Users"" (""Email"");
            CREATE INDEX ""IX_Users_IsDeleted_DeletedAt"" ON ""Users"" (""IsDeleted"", ""DeletedAt"");
            CREATE INDEX ""IX_Comments_UserId"" ON ""Comments"" (""UserId"");
            CREATE INDEX ""IX_Comments_IsDeleted_DeletedAt"" ON ""Comments"" (""IsDeleted"", ""DeletedAt"");
            CREATE INDEX ""IX_Comments_IsDeleted_CreatedAt_Id"" ON ""Comments"" (""IsDeleted"", ""CreatedAt"", ""Id"");
            CREATE INDEX ""IX_Comments_IsDeleted_UpdatedAt_Id"" ON ""Comments"" (""IsDeleted"", ""UpdatedAt"", ""Id"");
            CREATE INDEX ""IX_Comments_IsDeleted_UserId_Id"" ON ""Comments"" (""IsDeleted"", ""UserId"", ""Id"");
            ALTER TABLE ""Comments"" ADD CONSTRAINT ""FK_Comments_Users_UserId"" 
                FOREIGN KEY (""UserId"") REFERENCES ""Users"" (""Id"") ON DELETE CASCADE;
            ANALYZE ""Users"";
            ANALYZE ""Comments"";", connection))
        {
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}