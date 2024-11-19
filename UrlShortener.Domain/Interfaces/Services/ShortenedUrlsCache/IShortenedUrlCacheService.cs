using UrlShortener.Domain.Models;

namespace UrlShortener.Domain.Interfaces.Services.ShortenedUrlsCache;

public interface IShortenedUrlCacheService : ITopAccessUrlCacheService
{
    Task AddUrlAsync(ShortenedUrlCacheItem url);
    Task<ShortenedUrlCacheItem?> GetFromShortCodeAsync(string shortCode);
    Task<bool> TryUpdateAccessStatus(string shortCode);
}