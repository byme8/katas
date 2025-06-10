using Bogus;
using MongoDB.Bson;
using MongoDB.Driver;
using Pagination.Data.MongoEntities;

namespace Pagination.Data;

public class PaginationMongoContext(IMongoClient client, ILogger<PaginationMongoContext> logger)
{
    private readonly IMongoDatabase _database = client.GetDatabase("paginationdb");

    public IMongoCollection<User> Users => _database.GetCollection<User>("users");
    public IMongoCollection<Comment> Comments => _database.GetCollection<Comment>("comments");

    public async Task SeedDataAsync()
    {
        try
        {
            var usersCount = await Users.CountDocumentsAsync(FilterDefinition<User>.Empty);
            if (usersCount > 0)
            {
                logger.LogInformation("Database already seeded with {Count} users", usersCount);
                return;
            }

            logger.LogInformation("Starting database seeding...");

            var names = new Bogus.DataSets.Name();
            var messages = new Bogus.DataSets.Lorem();
            Randomizer.Seed = new Random(42);

            var users = new List<User>();
            var comments = new List<Comment>();

            var howManyUserToGenerate = 100_000;
            var howManyCommentsToGenerate = 1000;
            for (int i = 0; i < howManyUserToGenerate; i++)
            {
                var name = names.FullName();
                var user = new User
                {
                    Id = ObjectId.GenerateNewId(),
                    Name = name,
                    Email = $"{name.ToLowerInvariant().Replace(" ", ".")}@example.com",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                users.Add(user);

                for (int j = 0; j < howManyCommentsToGenerate; j++)
                {
                    var message = messages.Sentences();
                    var comment = new Comment
                    {
                        Id = ObjectId.GenerateNewId(),
                        UserId = user.Id,
                        Message = message,
                        CreatedAt = DateTime.UtcNow.AddMinutes(-Random.Shared.Next(0, 10000)),
                        UpdatedAt = DateTime.UtcNow.AddMinutes(-Random.Shared.Next(0, 10000))
                    };
                    comments.Add(comment);
                }

                if (users.Count >= 1000)
                {
                    await Users.InsertManyAsync(users);
                    await Comments.InsertManyAsync(comments);
                    logger.LogInformation("Inserted batch {Batch} of users and comments", i / 1000);
                    users.Clear();
                    comments.Clear();
                }
            }

            if (users.Any())
            {
                await Users.InsertManyAsync(users);
                await Comments.InsertManyAsync(comments);
            }

            await CreateIndexesAsync();

            logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during database seeding");
            throw;
        }
    }

    private async Task CreateIndexesAsync()
    {
        var userIndexKeys = Builders<User>.IndexKeys;
        var userIndexModels = new List<CreateIndexModel<User>>
        {
            new CreateIndexModel<User>(userIndexKeys.Ascending(u => u.Email)),
            new CreateIndexModel<User>(userIndexKeys.Ascending(u => u.IsDeleted).Ascending(u => u.DeletedAt))
        };
        await Users.Indexes.CreateManyAsync(userIndexModels);

        var commentIndexKeys = Builders<Comment>.IndexKeys;
        var commentIndexModels = new List<CreateIndexModel<Comment>>
        {
            new CreateIndexModel<Comment>(commentIndexKeys.Ascending(c => c.UserId)),
            new CreateIndexModel<Comment>(commentIndexKeys.Ascending(c => c.IsDeleted).Ascending(c => c.DeletedAt))
        };
        await Comments.Indexes.CreateManyAsync(commentIndexModels);

        logger.LogInformation("Database indexes created successfully");
    }
}