# nullable disable

using UrlShortener.Domain.Entities;

namespace UrlShortener.Domain.Models;

public class ShortenedUrlCacheItem
{
    public int Id { get; set; }
    public string OriginalUrl { get; set; }
    public string ShortCode { get; set; }
    public int ClickCount { get; set; }
    public long LastAccessTime
    {
        get => _lastAccessTime ?? -1;
        set => _lastAccessTime = value;
    }

    public static ShortenedUrlCacheItem FromEntity(ShortenedUrl shortenedUrl)
        => new()
        {
            Id = shortenedUrl.Id,
            OriginalUrl = shortenedUrl.OriginalUrl,
            ShortCode = shortenedUrl.ShortCode,
            ClickCount = shortenedUrl.ClickCount,
            LastAccessTime = shortenedUrl.LastAccessTime?.Ticks ?? -1
        };

    public override string ToString()
    {
        return $"{OriginalUrl} -> {ShortCode} : {ClickCount} ({LastAccessTime})";
    }

    private long? _lastAccessTime;
}