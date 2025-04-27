using Microsoft.EntityFrameworkCore;
using TinyUrl.Data;
using TinyUrl.Handlers;
using TinyUrl.Services;

namespace TinyUrl;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var services = builder.Services;

        services.AddHttpContextAccessor();
        services.AddMemoryCache();
        services.AddDbContext<TinyUrlDbContext>(o => { o.UseSqlite("Data Source=tinyurl.db"); });
        services.AddScoped<TinyUrlService>();

        var app = builder.Build();

        app.MapTinyUrlRedirect();
        app.MapAddUrl();

        app.Bootstrap();

        app.Run();
    }
}