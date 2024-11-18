using UrlShortener.Domain.Models;

namespace UrlShortener.Domain.Interfaces.Services.ShortenedUrlsCache;

public interface ITopAccessUrlCacheService
{
    Task<List<ShortenedUrlCacheItem>> GetTopUrlsAsync(int count);
    Task RefreshTopUrlsAsync(IList<ShortenedUrlCacheItem> urls);
}