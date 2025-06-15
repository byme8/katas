using Bogus;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Pagination.Data;
using Pagination.Data.Entities;
using System.Data;
using System.Diagnostics;
using System.Globalization;

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
            var companiesCount = await context.Companies.CountAsync(cancellationToken);
            if (companiesCount > 0)
            {
                logger.LogInformation("Database already seeded with {Count} companies", companiesCount);
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
        var faker = new Faker();
        Randomizer.Seed = new Random(42);

        const int totalCompanies = 100_000;
        const int minUsersPerCompany = 25;
        const int maxUsersPerCompany = 500;
        const int batchSize = 10_000;

        var baseTime = DateTime.UtcNow;

        // Special small company details
        const long smallCompanyId = 1;
        const string smallCompanyName = "Small Company";
        const int smallCompanyUserCount = 15;


        // Bulk insert companies
        logger.LogInformation("Starting bulk insert of {Count} companies plus Small Company", totalCompanies);
        await using (var writer = await connection.BeginBinaryImportAsync(
                         @"COPY ""Companies"" (""Id"", ""Name"", ""Email"", ""CreatedAt"", ""UpdatedAt"", ""IsDeleted"", ""DeletedAt"") FROM STDIN (FORMAT BINARY)",
                         cancellationToken))
        {
            // First, insert the special small company
            await writer.StartRowAsync(cancellationToken);
            await writer.WriteAsync(smallCompanyId, NpgsqlTypes.NpgsqlDbType.Bigint, cancellationToken);
            await writer.WriteAsync(smallCompanyName, NpgsqlTypes.NpgsqlDbType.Text, cancellationToken);
            await writer.WriteAsync("contact@smallcompany.com", NpgsqlTypes.NpgsqlDbType.Text, cancellationToken);
            await writer.WriteAsync(baseTime, NpgsqlTypes.NpgsqlDbType.TimestampTz, cancellationToken);
            await writer.WriteAsync(baseTime, NpgsqlTypes.NpgsqlDbType.TimestampTz, cancellationToken);
            await writer.WriteAsync(false, NpgsqlTypes.NpgsqlDbType.Boolean, cancellationToken);
            await writer.WriteAsync(DBNull.Value, NpgsqlTypes.NpgsqlDbType.TimestampTz, cancellationToken);

            // Then insert the regular companies
            for (int i = 0; i < totalCompanies; i++)
            {
                var companyName = GenerateCompanyName(faker);
                var companyId = i + 2; // Start from 2 since 1 is the small company
                var emailDomain = companyName.ToLowerInvariant()
                    .Replace(" ", "")
                    .Replace(".", "")
                    .Replace(",", "")
                    .Replace("'", "")
                    .Replace("&", "")
                    .Replace("-", "");

                await writer.StartRowAsync(cancellationToken);
                await writer.WriteAsync(companyId, NpgsqlTypes.NpgsqlDbType.Bigint, cancellationToken);
                await writer.WriteAsync(companyName, NpgsqlTypes.NpgsqlDbType.Text, cancellationToken);
                await writer.WriteAsync($"contact@{emailDomain}.com", NpgsqlTypes.NpgsqlDbType.Text, cancellationToken);
                await writer.WriteAsync(baseTime, NpgsqlTypes.NpgsqlDbType.TimestampTz, cancellationToken);
                await writer.WriteAsync(baseTime, NpgsqlTypes.NpgsqlDbType.TimestampTz, cancellationToken);
                await writer.WriteAsync(false, NpgsqlTypes.NpgsqlDbType.Boolean, cancellationToken);
                await writer.WriteAsync(DBNull.Value, NpgsqlTypes.NpgsqlDbType.TimestampTz, cancellationToken);

                if ((i + 1) % 10000 == 0)
                {
                    logger.LogInformation("Inserted {Count} companies", i + 1);
                }
            }

            await writer.CompleteAsync(cancellationToken);
        }

        // Get all company IDs for users
        var companyIds = new List<long>();
        await using (var cmd = new NpgsqlCommand(@"SELECT ""Id"" FROM ""Companies""", connection))
        await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                companyIds.Add(reader.GetInt64(0));
            }
        }

        // Bulk insert users in batches
        logger.LogInformation("Starting bulk insert of users (random {Min}-{Max} per company) plus {SmallCount} for Small Company",
            minUsersPerCompany, maxUsersPerCompany, smallCompanyUserCount);
        var userCount = 0L;

        // First, insert users for the small company
        await using (var writer = await connection.BeginBinaryImportAsync(
                         @"COPY ""Users"" (""Id"", ""CompanyId"", ""Name"", ""Email"", ""PhoneNumber"", ""TwitterHandle"", ""FacebookProfile"", ""WhatsAppNumber"", ""InstagramHandle"", ""BlueskyHandle"", ""CreatedAt"", ""UpdatedAt"", ""IsDeleted"", ""DeletedAt"") FROM STDIN (FORMAT BINARY)",
                         cancellationToken))
        {
            for (int i = 0; i < smallCompanyUserCount; i++)
            {
                var name = names.FullName();
                var createdAt = baseTime.AddMinutes(-Random.Shared.Next(0, 10000));
                var updatedAt = baseTime.AddMinutes(-Random.Shared.Next(0, 10000));
                var socialFields = GenerateRandomSocialFields(faker, name);

                await writer.StartRowAsync(cancellationToken);
                await writer.WriteAsync(++userCount, NpgsqlTypes.NpgsqlDbType.Bigint, cancellationToken);
                await writer.WriteAsync(smallCompanyId, NpgsqlTypes.NpgsqlDbType.Bigint, cancellationToken);
                await writer.WriteAsync(name, NpgsqlTypes.NpgsqlDbType.Text, cancellationToken);
                await writer.WriteAsync($"{name.ToLowerInvariant().Replace(" ", ".")}@smallcompany.com", NpgsqlTypes.NpgsqlDbType.Text, cancellationToken);
                await writer.WriteAsync(socialFields.PhoneNumber ?? (object)DBNull.Value, NpgsqlTypes.NpgsqlDbType.Text, cancellationToken);
                await writer.WriteAsync(socialFields.TwitterHandle ?? (object)DBNull.Value, NpgsqlTypes.NpgsqlDbType.Text, cancellationToken);
                await writer.WriteAsync(socialFields.FacebookProfile ?? (object)DBNull.Value, NpgsqlTypes.NpgsqlDbType.Text, cancellationToken);
                await writer.WriteAsync(socialFields.WhatsAppNumber ?? (object)DBNull.Value, NpgsqlTypes.NpgsqlDbType.Text, cancellationToken);
                await writer.WriteAsync(socialFields.InstagramHandle ?? (object)DBNull.Value, NpgsqlTypes.NpgsqlDbType.Text, cancellationToken);
                await writer.WriteAsync(socialFields.BlueskyHandle ?? (object)DBNull.Value, NpgsqlTypes.NpgsqlDbType.Text, cancellationToken);
                await writer.WriteAsync(createdAt, NpgsqlTypes.NpgsqlDbType.TimestampTz, cancellationToken);
                await writer.WriteAsync(updatedAt, NpgsqlTypes.NpgsqlDbType.TimestampTz, cancellationToken);
                await writer.WriteAsync(false, NpgsqlTypes.NpgsqlDbType.Boolean, cancellationToken);
                await writer.WriteAsync(DBNull.Value, NpgsqlTypes.NpgsqlDbType.TimestampTz, cancellationToken);
            }

            await writer.CompleteAsync(cancellationToken);
        }

        logger.LogInformation("Inserted {Count} users for Small Company", smallCompanyUserCount);

        // Then insert users for regular companies in batches
        var batchNumber = 0;
        var totalBatches = (int)Math.Ceiling((double)totalCompanies / batchSize);
        
        for (int batch = 0; batch < totalCompanies; batch += batchSize)
        {
            batchNumber++;
            var companyBatch = companyIds.Skip(batch + 1).Take(batchSize).ToList(); // Skip small company (ID 1)
            var batchStartTime = DateTime.UtcNow;
            
            logger.LogInformation("Starting batch {BatchNumber}/{TotalBatches} - Processing {CompanyCount} companies", 
                batchNumber, totalBatches, companyBatch.Count);

            await using var writer = await connection.BeginBinaryImportAsync(
                @"COPY ""Users"" (""Id"", ""CompanyId"", ""Name"", ""Email"", ""PhoneNumber"", ""TwitterHandle"", ""FacebookProfile"", ""WhatsAppNumber"", ""InstagramHandle"", ""BlueskyHandle"", ""CreatedAt"", ""UpdatedAt"", ""IsDeleted"", ""DeletedAt"") FROM STDIN (FORMAT BINARY)",
                cancellationToken);

            var usersInThisBatch = 0;
            var companiesProcessed = 0;
            
            foreach (var companyId in companyBatch)
            {
                var usersForThisCompany = Random.Shared.Next(minUsersPerCompany, maxUsersPerCompany + 1);
                
                for (int j = 0; j < usersForThisCompany; j++)
                {
                    var name = names.FullName();
                    var createdAt = baseTime.AddMinutes(-Random.Shared.Next(0, 10000));
                    var updatedAt = baseTime.AddMinutes(-Random.Shared.Next(0, 10000));
                    var socialFields = GenerateRandomSocialFields(faker, name);

                    await writer.StartRowAsync(cancellationToken);
                    await writer.WriteAsync(++userCount, NpgsqlTypes.NpgsqlDbType.Bigint, cancellationToken);
                    await writer.WriteAsync(companyId, NpgsqlTypes.NpgsqlDbType.Bigint, cancellationToken);
                    await writer.WriteAsync(name, NpgsqlTypes.NpgsqlDbType.Text, cancellationToken);
                    await writer.WriteAsync($"{name.ToLowerInvariant().Replace(" ", ".")}@example.com", NpgsqlTypes.NpgsqlDbType.Text, cancellationToken);
                    await writer.WriteAsync(socialFields.PhoneNumber ?? (object)DBNull.Value, NpgsqlTypes.NpgsqlDbType.Text, cancellationToken);
                    await writer.WriteAsync(socialFields.TwitterHandle ?? (object)DBNull.Value, NpgsqlTypes.NpgsqlDbType.Text, cancellationToken);
                    await writer.WriteAsync(socialFields.FacebookProfile ?? (object)DBNull.Value, NpgsqlTypes.NpgsqlDbType.Text, cancellationToken);
                    await writer.WriteAsync(socialFields.WhatsAppNumber ?? (object)DBNull.Value, NpgsqlTypes.NpgsqlDbType.Text, cancellationToken);
                    await writer.WriteAsync(socialFields.InstagramHandle ?? (object)DBNull.Value, NpgsqlTypes.NpgsqlDbType.Text, cancellationToken);
                    await writer.WriteAsync(socialFields.BlueskyHandle ?? (object)DBNull.Value, NpgsqlTypes.NpgsqlDbType.Text, cancellationToken);
                    await writer.WriteAsync(createdAt, NpgsqlTypes.NpgsqlDbType.TimestampTz, cancellationToken);
                    await writer.WriteAsync(updatedAt, NpgsqlTypes.NpgsqlDbType.TimestampTz, cancellationToken);
                    await writer.WriteAsync(false, NpgsqlTypes.NpgsqlDbType.Boolean, cancellationToken);
                    await writer.WriteAsync(DBNull.Value, NpgsqlTypes.NpgsqlDbType.TimestampTz, cancellationToken);
                    
                    usersInThisBatch++;
                }
                
                companiesProcessed++;
                
                // Progress update every 1000 companies within a batch
                if (companiesProcessed % 1000 == 0)
                {
                    logger.LogInformation("  ↳ Batch {BatchNumber}: Processed {CompaniesProcessed}/{TotalInBatch} companies, {UsersInBatch} users so far", 
                        batchNumber, companiesProcessed, companyBatch.Count, usersInThisBatch);
                }
            }

            await writer.CompleteAsync(cancellationToken);
            
            var batchDuration = DateTime.UtcNow - batchStartTime;
            var usersPerSecond = usersInThisBatch / Math.Max(batchDuration.TotalSeconds, 1);
            
            logger.LogInformation("✓ Completed batch {BatchNumber}/{TotalBatches}: {UsersInBatch} users for {CompaniesProcessed} companies in {Duration:F1}s ({UsersPerSecond:F0} users/sec). Total users: {TotalUsers}",
                batchNumber, totalBatches, usersInThisBatch, companiesProcessed, batchDuration.TotalSeconds, usersPerSecond, userCount);
        }

    }


    private static string GenerateCompanyName(Faker faker)
    {
        return faker.PickRandom(
            // Tech companies
            $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(faker.Hacker.Adjective())} {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(faker.Hacker.Noun())}",
            $"{faker.Name.LastName()} {faker.PickRandom("Technologies", "Systems", "Solutions", "Software", "Digital", "Labs", "Dynamics")}",
            $"{faker.Address.City()} {faker.PickRandom("Tech", "Digital", "Systems", "Solutions", "Software")}",

            // Traditional companies
            $"{faker.Name.LastName()} & {faker.Name.LastName()}",
            $"{faker.Name.LastName()} {faker.PickRandom("Corp", "Inc", "LLC", "Group", "Holdings", "Enterprises", "Industries")}",
            $"{faker.Address.City()} {faker.PickRandom("Manufacturing", "Industries", "Corp", "Group", "Holdings")}",

            // Creative companies
            $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(faker.Commerce.Color())} {faker.PickRandom("Creative", "Design", "Studio", "Media", "Productions")}",
            $"{faker.PickRandom("Alpha", "Beta", "Gamma", "Delta", "Omega", "Prime", "Elite", "Global")} {faker.Commerce.Department()}",

            // Services
            $"{faker.Address.City()} {faker.PickRandom("Consulting", "Services", "Partners", "Advisory", "Associates")}",
            $"{faker.Name.LastName()} {faker.PickRandom("Consulting", "Partners", "Advisory", "Associates", "Group")}",

            // Finance/Professional
            $"{faker.Name.LastName()} {faker.PickRandom("Capital", "Financial", "Investments", "Wealth Management", "Banking")}",
            $"{faker.Address.City()} {faker.PickRandom("Law", "Legal", "Attorneys", "Partners", "Associates")}",

            // Retail/Commerce
            $"{faker.Commerce.ProductName()} {faker.PickRandom("Co", "Company", "Store", "Retail", "Marketplace")}",
            $"{faker.Address.City()} {faker.PickRandom("Motors", "Auto", "Dealership", "Sales", "Automotive")}",

            // Healthcare/Medical
            $"{faker.Address.City()} {faker.PickRandom("Medical", "Health", "Healthcare", "Clinic", "Hospital")}",
            $"{faker.Name.LastName()} {faker.PickRandom("Medical Group", "Healthcare", "Clinic", "Associates")}"
        );
    }

    private static SocialFields GenerateRandomSocialFields(Faker faker, string name)
    {
        var socialFields = new SocialFields();
        var fieldsToFill = Random.Shared.Next(1, 3); // Fill 1 or 2 fields randomly
        var availableFields = new List<Action>
        {
            () => socialFields.PhoneNumber = faker.Phone.PhoneNumber(),
            () => socialFields.TwitterHandle = $"@{faker.Internet.UserName()}",
            () => socialFields.FacebookProfile = $"https://facebook.com/{faker.Internet.UserName()}",
            () => socialFields.WhatsAppNumber = faker.Phone.PhoneNumber(),
            () => socialFields.InstagramHandle = $"@{faker.Internet.UserName()}",
            () => socialFields.BlueskyHandle = $"@{faker.Internet.UserName()}.bsky.social"
        };

        // Randomly select which fields to fill
        var fieldsToSet = availableFields.OrderBy(x => Random.Shared.Next()).Take(fieldsToFill);
        foreach (var fieldSetter in fieldsToSet)
        {
            fieldSetter();
        }

        return socialFields;
    }
}

internal class SocialFields
{
    public string? PhoneNumber { get; set; }
    public string? TwitterHandle { get; set; }
    public string? FacebookProfile { get; set; }
    public string? WhatsAppNumber { get; set; }
    public string? InstagramHandle { get; set; }
    public string? BlueskyHandle { get; set; }
}