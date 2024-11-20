# nullable disable

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

using UrlShortener.Domain.Entities;
using UrlShortener.Infrastructure.Databases.Configurations;


namespace UrlShortener.Infrastructure.Databases;

public class ShortUrlDbContext : DbContext
{
    public virtual DbSet<ShortenedUrl> ShortenedUrls { get; set; }

    public ShortUrlDbContext() { }
    public ShortUrlDbContext(DbContextOptions<ShortUrlDbContext> options)
            : base(options) { }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new ShortenedUrlConfiguration());
    }

}