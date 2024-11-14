using UrlShortener.Domain.Entities;

namespace UrlShortener.Domain.Interfaces.Services;

public interface IShortenedUrlCacheService : ITopAccessUrlCacheService {
    Task AddUrlAsync(ShortenedUrl url); 
    Task<ShortenedUrl?> GetFromShortUrlAsync(string shortUrl, bool isPrefixIncluded);
    Task<ShortenedUrl?> GetFromUrlIdAsync(int urlId, bool isPrefixIncluded);
}