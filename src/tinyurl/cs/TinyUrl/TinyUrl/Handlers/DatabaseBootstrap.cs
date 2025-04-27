using TinyUrl.Data;

namespace TinyUrl.Handlers;

public static class DatabaseBootstrap
{
    public static void Bootstrap(this WebApplication app)
    {
        var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<TinyUrlDbContext>();
        context.Database.EnsureCreated();

        scope.Dispose();
    }
}