using Pagination.Data;

namespace Pagination.Services;

public class DatabaseSeedingService(IServiceProvider serviceProvider, ILogger<DatabaseSeedingService> logger)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting database seeding service");

        using var scope = serviceProvider.CreateScope();
        var mongoContext = scope.ServiceProvider.GetRequiredService<PaginationMongoContext>();
        
        await mongoContext.SeedDataAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}