using Microsoft.EntityFrameworkCore;
using TinyUrl.Data.Tables;

namespace TinyUrl.Data;

public class TinyUrlDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<UrlEntity> Urls { get; set; }
}