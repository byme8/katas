using Microsoft.Extensions.Caching.Memory;
using TinyUrl.Data;
using TinyUrl.Data.Tables;

namespace TinyUrl.Services;

public class TinyUrlService(TinyUrlDbContext context, IMemoryCache cache)
{
    public async Task<string> AddUrl(string host, string url)
    {
        var entity = await context.Urls.AddAsync(new UrlEntity
        {
            LongUrl = url
        });

        await context.SaveChangesAsync();

        var entityId = entity.Entity.Id;
        var encodedId = IdFormatter.Encoder.Encode(entityId);

        entity.Entity.EncodedId = encodedId;

        await context.SaveChangesAsync();

        return $"{host}/{encodedId}";
    }

    public async Task<string?> GetUrl(string path)
    {
        return await cache.GetOrCreateAsync(path, async entry =>
        {
            var cleanPath = path.Trim('/');
            var decoded = IdFormatter.Encoder.Decode(cleanPath);
            var id = decoded.First();

            var entity = await context.Urls.FindAsync(id);

            if (entity == null)
            {
                entry.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(1);
                return null;
            }
            
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            
            return entity.LongUrl;
        });
    }
}