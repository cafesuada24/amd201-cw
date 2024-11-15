using UrlShortener.Domain.Entities;

namespace UrlShortener.Domain.Interfaces.Services;

public interface IShortenedUrlCacheService : ITopAccessUrlCacheService {
    Task AddUrlAsync(ShortenedUrl url); 
    Task<ShortenedUrl?> GetFromShortCodeAsync(string shortCode, bool isPrefixIncluded=false);
    Task<ShortenedUrl?> GetFromUrlIdAsync(int urlId, bool isPrefixIncluded=false);
}