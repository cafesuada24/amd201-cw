using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Infrastructure.Databases.Configurations;
public class ShortenedUrlConfiguration : IEntityTypeConfiguration<ShortenedUrl>
{
    public void Configure(EntityTypeBuilder<ShortenedUrl> builder)
    {
        builder.ToTable("ShortenedUrls");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.OriginalUrl)
            .IsRequired()
            .HasConversion(v => v.ToString(), v => new Uri(v))
            .HasMaxLength(2048);

        builder.Property(e => e.ShortUrl)
            .IsRequired()
            .HasConversion(v => v.ToString(), v => new Uri(v))
            .HasMaxLength(2048);

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.ExpireAt)
            .IsRequired(false);

        builder.Property(e => e.LastAccessTime)
            .IsRequired(false);

        builder.Property(e => e.ClickCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.Status)
            .IsRequired();
    }
}

