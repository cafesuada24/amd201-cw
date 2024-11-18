using UrlShortener.Domain.Entities;

namespace UrlShortener.Domain.Interfaces.Services.ShortenedUrls;

public interface IShortenedUrlWrite
{
    Task UpdateAccessStatus(string shortCode);

    // Task Update(ShortenedUrl shortenedUrl);
    // Task Add(ShortenedUrl shortenedUrl);
    Task<ShortenedUrl> CreateShortUrl(string url);
    Task Delete(ShortenedUrl shortenedUrl);
}