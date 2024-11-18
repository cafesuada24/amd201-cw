# nullable disable

using UrlShortener.Domain.Entities;

namespace UrlShortener.Domain.Models;

public class ShortenedUrlCacheItem
{
    public string OriginalUrl { get; set; }
    public string ShortCode { get; set; }
    public int ClickCount { get; set; }
    public long? LastAccessTime
    {
        get => _lastAccessTime ?? -1;
        set => _lastAccessTime = value;
    }

    public static ShortenedUrlCacheItem FromEntity(ShortenedUrl shortenedUrl)
        => new()
        {
            OriginalUrl = shortenedUrl.OriginalUrl,
            ShortCode = shortenedUrl.ShortCode,
            ClickCount = shortenedUrl.ClickCount,
            LastAccessTime = shortenedUrl.LastAccessTime?.Ticks
        };

    public override string ToString()
    {
        return $"{OriginalUrl} -> {ShortCode} : {ClickCount} ({LastAccessTime})";
    }

    private long? _lastAccessTime;
}