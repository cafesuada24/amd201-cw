using UrlShortener.Domain.Entities;

namespace UrlShortener.Domain.Interfaces.Services;

public interface ITopAccessUrlCacheService {
    Task<List<ShortenedUrl>> GetTopUrlsAsync(int count);
    Task RefreshCacheAsync(List<ShortenedUrl> urls);
}