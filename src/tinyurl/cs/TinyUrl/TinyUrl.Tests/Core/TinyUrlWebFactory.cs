using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TinyUrl.Data;

namespace TinyUrl.Tests.Core;

public class TinyUrlWebFactory : WebApplicationFactory<TinyUrl.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var guid = Guid.NewGuid().ToString();
            var tempPath = Path.Combine(Path.GetTempPath(), guid);
            services.AddDbContext<TinyUrlDbContext>(o => { o.UseSqlite($"Data Source={tempPath}.db"); });
        });

        base.ConfigureWebHost(builder);
    }
}